// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
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
    using Catel.IoC;
    using Catel.Logging;
    using Catel.Reflection;
    using Catel.Services;
    using Catel.Threading;
    using FileSystem;
    using MethodTimer;
    using SystemInfo;

    public class SupportPackageService : ISupportPackageService
    {
        private const int DirectorySizeLimitInBytes = 25 * 1024 * 1024;
        private const string CustomDataFolderName = "CustomData";

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IScreenCaptureService _screenCaptureService;
        private readonly ITypeFactory _typeFactory;
        private readonly IDirectoryService _directoryService;
        private readonly IAppDataService _appDataService;
        private readonly IEncryptionService _encryptionService;
        private readonly IFileService _fileService;
        private readonly ISystemInfoService _systemInfoService;

        public SupportPackageService(ISystemInfoService systemInfoService, IScreenCaptureService screenCaptureService, ITypeFactory typeFactory,
            IDirectoryService directoryService, IAppDataService appDataService, IEncryptionService encryptionService, IFileService fileService)
        {
            Argument.IsNotNull(() => systemInfoService);
            Argument.IsNotNull(() => screenCaptureService);
            Argument.IsNotNull(() => typeFactory);
            Argument.IsNotNull(() => directoryService);
            Argument.IsNotNull(() => appDataService);
            Argument.IsNotNull(() => encryptionService);
            Argument.IsNotNull(() => fileService);

            _systemInfoService = systemInfoService;
            _screenCaptureService = screenCaptureService;
            _typeFactory = typeFactory;
            _directoryService = directoryService;
            _appDataService = appDataService;
            _encryptionService = encryptionService;
            _fileService = fileService;
        }

        public virtual async Task<bool> CreateSupportPackageAsync(string zipFileName, string[] directories, string[] excludeFileNamePatterns)
        {
            using (var packageContext = new SupportPackageContext
            {
                ZipFileName = zipFileName,
            })
            {
                packageContext.AddExcludeFileNamePatterns(excludeFileNamePatterns);

                return await CreateSupportPackageAsync(packageContext);
            }
        }

        [Time]
        public virtual async Task<bool> CreateSupportPackageAsync(SupportPackageContext supportPackageContext)
        {
            Argument.IsNotNullOrEmpty(() => supportPackageContext.ZipFileName);

            var result = true;

            try
            {
                Log.Info("Creating support package");

                // Note: screenshot first, see remarks in screenshot method
                var screenshotFileName = supportPackageContext.GetFile("screenshot.jpg");
                await CaptureWindowAndSaveAsync(screenshotFileName);

                var systemInfoXmlFileName = supportPackageContext.GetFile("systeminfo.xml");
                var systemInfoTxtFileName = supportPackageContext.GetFile("systeminfo.txt");
                await GetAndSaveSystemInformationAsync(systemInfoXmlFileName, systemInfoTxtFileName);

                var supportPackageProviderTypes = (from type in TypeCache.GetTypes()
                                                   where !type.IsAbstractEx() && type.IsClassEx() &&
                                                         type.ImplementsInterfaceEx<ISupportPackageProvider>()
                                                   select type).ToList();

                foreach (var supportPackageProviderType in supportPackageProviderTypes)
                {
                    try
                    {
                        Log.Debug("Gathering support package info from '{0}'", supportPackageProviderType.FullName);

                        var provider = (ISupportPackageProvider)_typeFactory.CreateInstance(supportPackageProviderType);
                        await provider.ProvideAsync(supportPackageContext);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to gather support package info from '{0}'. Info will be excluded from the package", supportPackageProviderType.FullName);
                    }
                }

                if (supportPackageContext.IsEncrypted)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var fileStream = new FileStream(supportPackageContext.ZipFileName, FileMode.OpenOrCreate))
                        {
                            await ZipSupportPackageContentAsync(memoryStream, supportPackageContext);

                            if (supportPackageContext.CustomFileSystemPaths.Any())
                            {
                                await ZipCustomAddedContentAsync(memoryStream, supportPackageContext.CustomFileSystemPaths, supportPackageContext.DescriptionBuilder ?? new StringBuilder());
                            }

                            await _encryptionService.EncryptAsync(memoryStream, fileStream, supportPackageContext.EncryptionContext);
                        }
                    }
                }
                else
                {
                    using (var fileStream = new FileStream(supportPackageContext.ZipFileName, FileMode.OpenOrCreate))
                    {
                        await ZipSupportPackageContentAsync(fileStream, supportPackageContext);
                        if (supportPackageContext.CustomFileSystemPaths.Any())
                        {
                            await ZipCustomAddedContentAsync(fileStream, supportPackageContext.CustomFileSystemPaths, supportPackageContext.DescriptionBuilder ?? new StringBuilder());
                        }
                    }
                }

                Log.Info("Support package created");
            }
            catch (Exception ex)
            {
                result = false;
                Log.Error(ex, "Error while creating support package");
            }

            return result;
        }

        private async Task ZipSupportPackageContentAsync(Stream stream, SupportPackageContext supportPackageContext)
        {
            var directories = supportPackageContext.ArtifactsDirectories;
            var excludeFileNamePatterns = supportPackageContext.ExcludeFileNamePatterns;

            using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Update, true))
            {
                zipArchive.CreateEntryFromDirectory(_appDataService.GetApplicationDataDirectory(Catel.IO.ApplicationDataTarget.UserRoaming), "AppData", CompressionLevel.Optimal);
                zipArchive.CreateEntryFromDirectory(supportPackageContext.RootDirectory, string.Empty, CompressionLevel.Optimal);

                if (directories is not null && directories.Count > 0)
                {
                    foreach (var directory in directories)
                    {
                        if (!_directoryService.Exists(directory))
                        {
                            Log.Warning($"Directory '{directory}' does not exist, skipping");
                            continue;
                        }

                        var directoryPathInArchive = directory.TrimEnd('\\').Split('\\').LastOrDefault();
                        if (!string.IsNullOrEmpty(directoryPathInArchive))
                        {
                            zipArchive.CreateEntryFromDirectory(directory, string.Empty, CompressionLevel.Optimal);
                        }
                    }
                }

                if (excludeFileNamePatterns is not null && excludeFileNamePatterns.Count > 0)
                {
                    Log.Info("Removing excluded files...");

                    var excludeFileNameRegexes = excludeFileNamePatterns.Select(s => new Regex(s.Replace("*", ".*").Replace(".", "\\.") + "$", RegexOptions.IgnoreCase | RegexOptions.Compiled)).ToList();

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

        private async Task ZipCustomAddedContentAsync(Stream stream, IEnumerable<string> customArtifactsDataPath, StringBuilder builder)
        {
            using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Update, true))
            {
                foreach (var path in customArtifactsDataPath)
                {
                    try
                    {
                        var directoryInfo = new DirectoryInfo(path);
                        if (directoryInfo.Exists)
                        {
                            var directorySize = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories).Sum(info => info.Length);
                            if (directorySize > DirectorySizeLimitInBytes)
                            {
                                Log.Info("Skipped directory '{0}' because its size is greater than '{1}' bytes", path, DirectorySizeLimitInBytes);

                                builder.AppendLine("- Directory (skipped): " + path);
                            }
                            else
                            {
                                zipArchive.CreateEntryFromDirectory(path, Path.Combine(CustomDataFolderName, directoryInfo.Name), CompressionLevel.Optimal);
                                builder.AppendLine("- Directory: " + path);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex);
                    }

                    try
                    {
                        if (_fileService.Exists(path))
                        {
                            zipArchive.CreateEntryFromAny(path, CustomDataFolderName, CompressionLevel.Optimal);
                            builder.AppendLine("- File: " + path);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex);
                    }

                }

                builder.AppendLine();
                builder.AppendLine("## File system entries");
                builder.AppendLine();
                builder.AppendLine("- Total: " + zipArchive.Entries.Count);
                builder.AppendLine("- Files: " + zipArchive.Entries.Count(entry => !entry.Name.EndsWith("/")));
                builder.AppendLine("- Directories: " + zipArchive.Entries.Count(entry => entry.Name.EndsWith("/")));

                var builderEntry = zipArchive.CreateEntry("SupportPackageOptions.txt");

                using (var streamWriter = new StreamWriter(builderEntry.Open()))
                {
                    await streamWriter.WriteAsync(builder.ToString());
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
                Log.Debug("Application.Current is null, cannot create screenshot");
                return;
            }

            var dispatcher = application.Dispatcher;

#pragma warning disable 4014
            dispatcher.BeginInvoke(new Action(() =>
            {
                var mainWindow = application.MainWindow;
                if (mainWindow is null)
                {
                    Log.Debug("Application.Current.MainWindow is null, cannot create screenshot");
                    return;
                }

                Log.Debug("Creating screenshot for support package");

                var image = _screenCaptureService.CaptureWindowImage(mainWindow);
                image.Save(screenshotFile, ImageFormat.Jpeg);
            }));
#pragma warning restore 4014
        }

        private Task GetAndSaveSystemInformationAsync(string xmlFileName, string textFileName)
        {
            Argument.IsNotNullOrEmpty(() => xmlFileName);
            Argument.IsNotNullOrEmpty(() => textFileName);

            return TaskHelper.Run(() =>
            {
                Log.Debug("Gathering system info for support package");

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
            }, true);
        }
    }
}
