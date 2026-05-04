namespace Orc.SupportPackage;

using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Catel;
using Catel.Collections;
using Catel.Reflection;
using Catel.Services;
using FileSystem;
using MethodTimer;
using Microsoft.Extensions.Logging;
using SystemInfo;

public class SupportPackageService : ISupportPackageService
{
    private readonly ILogger<SupportPackageService> _logger;
    private readonly ISystemInfoService _systemInfoService;
    private readonly IScreenCaptureService _screenCaptureService;
    private readonly IDirectoryService _directoryService;
    private readonly IFileService _fileService;
    private readonly IEntryAssemblyResolver _entryAssemblyResolver;
    private readonly IAppDataService _appDataService;
    private readonly IReadOnlyList<ISupportPackageProvider> _supportPackageProviders;
    private readonly IEncryptionService _encryptionService;

    public SupportPackageService(ILogger<SupportPackageService> logger, ISystemInfoService systemInfoService,
        IScreenCaptureService screenCaptureService, IDirectoryService directoryService, IFileService fileService,
        IEntryAssemblyResolver entryAssemblyResolver, IAppDataService appDataService,
        IEnumerable<ISupportPackageProvider> supportPackageProviders, IEncryptionService encryptionService)
    {
        _logger = logger;
        _systemInfoService = systemInfoService;
        _screenCaptureService = screenCaptureService;
        _directoryService = directoryService;
        _fileService = fileService;
        _entryAssemblyResolver = entryAssemblyResolver;
        _appDataService = appDataService;
        _supportPackageProviders = supportPackageProviders.ToArray();
        _encryptionService = encryptionService;
    }

    [Time]
    public virtual Task<bool> CreateSupportPackageAsync(string zipFileName, string[] directories, string[] excludeFileNamePatterns)
    {
        return CreateSupportPackageAsync(zipFileName, directories, excludeFileNamePatterns, null);
    }

    [Time]
    public virtual async Task<bool> CreateSupportPackageAsync(string zipFileName, string[] directories, string[] excludeFileNamePatterns, EncryptionContext? encryptionContext)
    {
        Argument.IsNotNullOrEmpty(() => zipFileName);

        var result = true;

        try
        {
            _logger.LogInformation("Creating support package");

            using (var supportPackageContext = new SupportPackageContext(_directoryService, _fileService, _entryAssemblyResolver))
            {
                // Note: screenshot first, see remarks in screenshot method
                var screenshotFileName = supportPackageContext.GetFile("screenshot.jpg");
                await CaptureWindowAndSaveAsync(screenshotFileName);

                var systemInfoJsonFileName = supportPackageContext.GetFile("systeminfo.json");
                var systemInfoTxtFileName = supportPackageContext.GetFile("systeminfo.txt");
                await GetAndSaveSystemInformationAsync(systemInfoJsonFileName, systemInfoTxtFileName);

                foreach (var supportPackageProvider in _supportPackageProviders)
                {
                    try
                    {
                        _logger.LogDebug("Gathering support package info from '{ProviderType}'", supportPackageProvider.GetType().FullName);

                        await supportPackageProvider.ProvideAsync(supportPackageContext);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to gather support package info from '{ProviderType}'. Info will be excluded from the package", supportPackageProvider.GetType().FullName);
                    }
                }

                if (encryptionContext is not null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await ZipSupportPackageContentAsync(memoryStream, supportPackageContext, directories, excludeFileNamePatterns);

                        await using (var fileStream = new FileStream(zipFileName, FileMode.OpenOrCreate))
                        {
                            await _encryptionService.EncryptAsync(memoryStream, fileStream, encryptionContext);
                        }
                    }
                }
                else
                {
                    await using (var fileStream = new FileStream(zipFileName, FileMode.OpenOrCreate))
                    {
                        await ZipSupportPackageContentAsync(fileStream, supportPackageContext, directories, excludeFileNamePatterns);
                    }
                }
            }

            _logger.LogInformation("Support package created");
        }
        catch (Exception ex)
        {
            result = false;
            _logger.LogError(ex, "Error while creating support package");
        }

        return result;
    }

    private async Task ZipSupportPackageContentAsync(Stream stream, SupportPackageContext supportPackageContext, string[] directories, string[] excludeFileNamePatterns)
    {
        using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Update, true))
        {
            zipArchive.CreateEntryFromDirectory(_appDataService.GetApplicationDataDirectory(Catel.IO.ApplicationDataTarget.UserRoaming), "AppData", CompressionLevel.Optimal);
            zipArchive.CreateEntryFromDirectory(supportPackageContext.RootDirectory, string.Empty, CompressionLevel.Optimal);

            if (directories is not null && directories.Length > 0)
            {
                foreach (var directory in directories)
                {
                    if (!_directoryService.Exists(directory))
                    {
                        _logger.LogWarning("Directory '{Directory}' does not exist, skipping", directory);
                        continue;
                    }

                    var directoryPathInArchive = directory.TrimEnd('\\').Split('\\').LastOrDefault();
                    if (!string.IsNullOrEmpty(directoryPathInArchive))
                    {
                        zipArchive.CreateEntryFromDirectory(directory, string.Empty, CompressionLevel.Optimal);
                    }
                }
            }

            if (excludeFileNamePatterns is not null && excludeFileNamePatterns.Length > 0)
            {
                _logger.LogInformation("Removing excluded files...");

                var excludeFileNameRegexes = excludeFileNamePatterns.Select(s => new Regex(s.Replace("*", ".*").Replace(".", "\\.") + "$",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1))).ToList();

                var zipEntries = zipArchive.Entries.ToList();
                foreach (var zipEntry in zipEntries)
                {
                    if (excludeFileNameRegexes.Any(regex => regex.IsMatch(zipEntry.FullName)))
                    {
                        zipEntry.Delete();
                    }
                }
            }

            await stream.FlushAsync();
        }
    }

    private async Task CaptureWindowAndSaveAsync(string screenshotFile)
    {
        Argument.IsNotNullOrEmpty(() => screenshotFile);

        // Note: we cannot use InvokeAsync here because it might cause deadlocks. Therefore we just dispatcher and 
        // hope it's ready before the package is created. Worst case it doesn't contain the screenshot.
        var application = Application.Current;
        if (application is null)
        {
            _logger.LogDebug("Application.Current is null, cannot create screenshot");
            return;
        }

        var dispatcher = application.Dispatcher;

#pragma warning disable 4014
        dispatcher.BeginInvoke(new Action(() =>
        {
            var mainWindow = application.MainWindow;
            if (mainWindow is null)
            {
                _logger.LogDebug("Application.Current.MainWindow is null, cannot create screenshot");
                return;
            }

            _logger.LogDebug("Creating screenshot for support package");

            var image = _screenCaptureService.CaptureWindowImage(mainWindow);
            image.Save(screenshotFile, ImageFormat.Jpeg);
        }));
#pragma warning restore 4014
    }

    private Task GetAndSaveSystemInformationAsync(string xmlFileName, string textFileName)
    {
        Argument.IsNotNullOrEmpty(() => xmlFileName);
        Argument.IsNotNullOrEmpty(() => textFileName);

        return Task.Run(() =>
        {
            _logger.LogDebug("Gathering system info for support package");

            var systemInfo = _systemInfoService.GetSystemInfo();

            // Xml
            var serializer = new XmlSerializer(systemInfo.GetType());
            using (var fileStream = new FileStream(xmlFileName, FileMode.OpenOrCreate))
            {
                serializer.Serialize(fileStream, systemInfo);
            }

            // Plain
            var stringBuilder = new StringBuilder();
            systemInfo.ForEach(x => stringBuilder.AppendLine(x.ToString()));
            File.WriteAllText(textFileName, stringBuilder.ToString());
        });
    }
}
