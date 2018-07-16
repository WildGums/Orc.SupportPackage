// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageBuilderService.cs" company="WildGums">
//   Copyright (c) 2008 - 2018 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Catel;
    using Catel.Logging;

    using Ionic.Zip;

    using Orc.FileSystem;

    public class SupportPackageBuilderService : ISupportPackageBuilderService
    {
        private const int DirectorySizeLimitInBytes = 25 * 1024 * 1024;

        #region Fields
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly ISupportPackageService _supportPackageService;

        private readonly IFileService _fileService;
        #endregion

        #region Constructors
        public SupportPackageBuilderService(ISupportPackageService supportPackageService, IFileService fileService)
        {
            Argument.IsNotNull(() => supportPackageService);
            Argument.IsNotNull(() => fileService);

            _supportPackageService = supportPackageService;
            _fileService = fileService;
        }

        #endregion

        #region Methods
        public async Task<bool> CreateSupportPackageAsync(string fileName, List<SupportPackageFileSystemArtifact> artifacts)
        {
            Argument.IsNotNullOrWhitespace(() => fileName);
            Argument.IsNotNull(() => artifacts);

            StringBuilder builder = new StringBuilder();
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

            var customDataDirectoryName = "CustomData";
            using (var zipFile = new ZipFile(fileName))
            {
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
                                    Log.Info("Skipped directory '{0}' beacuse its size is greater than '{1}' bytes", path, DirectorySizeLimitInBytes);

                                    builder.AppendLine("- Directory (skipped): " + path);
                                }
                                else
                                {
                                    zipFile.AddDirectory(path, Path.Combine(customDataDirectoryName, directoryInfo.Name));
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
                                zipFile.AddFile(path, customDataDirectoryName);
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
                builder.AppendLine("- Total: " + zipFile.Entries.Count);
                builder.AppendLine("- Files: " + zipFile.Entries.Count(entry => !entry.IsDirectory));
                builder.AppendLine("- Directories: " + zipFile.Entries.Count(entry => entry.IsDirectory));
                zipFile.AddEntry("SupportPackageOptions.txt", builder.ToString());
                zipFile.Save();
            }

            return result;
        }

        #endregion
    }
}
