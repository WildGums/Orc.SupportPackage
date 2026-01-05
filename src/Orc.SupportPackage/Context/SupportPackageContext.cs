namespace Orc.SupportPackage;

using System;
using System.IO;
using Catel;
using Catel.Logging;
using Catel.Reflection;
using Microsoft.Extensions.Logging;
using Orc.FileSystem;

public class SupportPackageContext : Disposable, ISupportPackageContext
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(SupportPackageContext));

    private readonly IDirectoryService _directoryService;
    private readonly IFileService _fileService;
    private readonly IEntryAssemblyResolver _entryAssemblyResolver;

    private readonly string _rootDirectory;

    public SupportPackageContext(IDirectoryService directoryService, IFileService fileService,
        IEntryAssemblyResolver entryAssemblyResolver)
    {
        _directoryService = directoryService;
        _fileService = fileService;
        _entryAssemblyResolver = entryAssemblyResolver;

        var assembly = _entryAssemblyResolver.Resolve();

        _rootDirectory = Path.Combine(Path.GetTempPath(), assembly.Company() ?? string.Empty, assembly.Title() ?? string.Empty,
            "support", DateTime.Now.ToString("yyyyMMdd_HHmmss"));

        _directoryService.Create(_rootDirectory);
    }

    public string RootDirectory { get { return _rootDirectory; } }

    public string GetDirectory(string relativeDirectoryName)
    {
        var fullPath = Path.Combine(_rootDirectory, relativeDirectoryName);

        _directoryService.Create(fullPath);

        return fullPath;
    }

    public string GetFile(string relativeFilePath)
    {
        var fullPath = Path.Combine(_rootDirectory, relativeFilePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (directory is not null)
        {
            _directoryService.Create(directory);
        }

        return fullPath;
    }

    protected override void DisposeManaged()
    {
        Logger.LogInformation("Deleting temporary files from '{0}'", _rootDirectory);

        try
        {
            _directoryService.Delete(_rootDirectory);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to delete temporary files");
        }
    }
}
