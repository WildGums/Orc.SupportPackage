namespace Orc.SupportPackage;

using System;
using System.IO;
using Catel;
using Catel.Logging;
using Catel.Reflection;

public class SupportPackageContext : Disposable, ISupportPackageContext
{
    private static readonly ILog Log = LogManager.GetCurrentClassLogger();

    private readonly string _rootDirectory;

    public SupportPackageContext()
    {
        var assembly = AssemblyHelper.GetRequiredEntryAssembly();

        _rootDirectory = Path.Combine(Path.GetTempPath(), assembly.Company() ?? string.Empty, assembly.Title() ?? string.Empty,
            "support", DateTime.Now.ToString("yyyyMMdd_HHmmss"));

        Directory.CreateDirectory(_rootDirectory);
    }

    public string RootDirectory { get { return _rootDirectory; } }

    public string GetDirectory(string relativeDirectoryName)
    {
        var fullPath = Path.Combine(_rootDirectory, relativeDirectoryName);

        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        return fullPath;
    }

    public string GetFile(string relativeFilePath)
    {
        var fullPath = Path.Combine(_rootDirectory, relativeFilePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (directory is not null)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        return fullPath;
    }

    protected override void DisposeManaged()
    {
        Log.Info("Deleting temporary files from '{0}'", _rootDirectory);

        try
        {
            if (Directory.Exists(_rootDirectory))
            {
                Directory.Delete(_rootDirectory, true);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete temporary files");
        }
    }
}
