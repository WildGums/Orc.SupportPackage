namespace Orc.SupportPackage;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel;
using Catel.Logging;
using FileSystem;

public class SupportPackageBuilderService : ISupportPackageBuilderService
{
    private const int DirectorySizeLimitInBytes = 25 * 1024 * 1024;

    private static readonly ILog Log = LogManager.GetCurrentClassLogger();

    private readonly ISupportPackageService _supportPackageService;

    private readonly IFileService _fileService;

    public SupportPackageBuilderService(ISupportPackageService supportPackageService, IFileService fileService)
    {
        ArgumentNullException.ThrowIfNull(supportPackageService);
        ArgumentNullException.ThrowIfNull(fileService);

        _supportPackageService = supportPackageService;
        _fileService = fileService;
    }

    public virtual async Task<bool> CreateSupportPackageAsync(string fileName, List<SupportPackageFileSystemArtifact> artifacts)
    {
        Argument.IsNotNullOrWhitespace(() => fileName);
        ArgumentNullException.ThrowIfNull(artifacts);

        var builder = new StringBuilder();
        builder.AppendLine("# Support package options");
        builder.AppendLine();
        builder.AppendLine("## Content providers");
        builder.AppendLine();

        foreach (var supportPackageFileSystemArtifact in artifacts)
        {
            builder.Append(supportPackageFileSystemArtifact.IncludeInSupportPackage ? "- [X] " : "- [ ] ");
            builder.AppendLine(supportPackageFileSystemArtifact.Title);
        }

        var excludeFileNamePatterns = artifacts.Where(artifact => !artifact.IncludeInSupportPackage).OfType<SupportPackageFileNamePattern>().SelectMany(artifact => artifact.FileNamePatterns).Distinct().ToArray();
        var directories = artifacts.Where(artifact => artifact.IncludeInSupportPackage).OfType<SupportPackageDirectory>().Select(artifact => artifact.DirectoryName).Distinct().ToArray();

        builder.AppendLine();
        builder.AppendLine("## Exclude file name patterns");
        builder.AppendLine();

        foreach (var excludeFileNamePattern in excludeFileNamePatterns)
        {
            builder.AppendLine("- " + excludeFileNamePattern);
        }

        builder.AppendLine();
        builder.AppendLine("## Include directories");
        builder.AppendLine();

        foreach (var directory in directories)
        {
            builder.AppendLine("- " + directory);
        }

        var result = await _supportPackageService.CreateSupportPackageAsync(fileName, directories, excludeFileNamePatterns);

        const string customDataDirectoryName = "CustomData";
        await using var fileStream = new FileStream(fileName, FileMode.OpenOrCreate);
        using var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Update);
           
        foreach (var artifact in artifacts.OfType<CustomPathsPackageFileSystemArtifact>().Where(artifact => artifact.IncludeInSupportPackage))
        {
            builder.AppendLine();
            builder.AppendLine("## Include custom data");
            builder.AppendLine();

            foreach (var path in artifact.Paths)
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
                            zipArchive.CreateEntryFromDirectory(path, Path.Combine(customDataDirectoryName, directoryInfo.Name), CompressionLevel.Optimal);
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
                        zipArchive.CreateEntryFromAny(path, customDataDirectoryName);
                        builder.AppendLine("- File: " + path);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex);
                }
            }
        }

        builder.AppendLine();
        builder.AppendLine("## File system entries");
        builder.AppendLine();
        builder.AppendLine("- Total: " + zipArchive.Entries.Count);
        builder.AppendLine("- Files: " + zipArchive.Entries.Count(entry => !entry.Name.EndsWith("/")));
        builder.AppendLine("- Directories: " + zipArchive.Entries.Count(entry => entry.Name.EndsWith("/")));

        var builderEntry = zipArchive.CreateEntry("SupportPackageOptions.txt");

#if NET10_0_OR_GREATER
        await using (var streamWriter = new StreamWriter(await builderEntry.OpenAsync()))
#else
        await using (var streamWriter = new StreamWriter(builderEntry.Open()))
#endif
        {
            await streamWriter.WriteAsync(builder.ToString());
        }

        await fileStream.FlushAsync();

        return result;
    }
}
