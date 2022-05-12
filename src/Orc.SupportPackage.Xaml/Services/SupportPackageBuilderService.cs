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
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Logging;
    using Orc.FileSystem;

    public class SupportPackageBuilderService : ISupportPackageBuilderService
    {
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
        public virtual async Task<bool> CreateSupportPackageAsync(string fileName, List<SupportPackageFileSystemArtifact> artifacts)
        {
            Argument.IsNotNullOrWhitespace(() => fileName);
            Argument.IsNotNull(() => artifacts);

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
            var directories = artifacts.Where(artifact => artifact.IncludeInSupportPackage).OfType<SupportPackageDirectory>().Select(artifact => artifact.DirectoryName)
                .Distinct()
                .ToArray();

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

            var customData = artifacts.Where(artifact => artifact.IncludeInSupportPackage).OfType<CustomPathsPackageFileSystemArtifact>().SelectMany(x => x.Paths);
            if (customData.Any())
            {
                builder.AppendLine();
                builder.AppendLine("## Include custom data");
                builder.AppendLine();
            }

            var result = await _supportPackageService.CreateSupportPackageAsync(fileName, directories, excludeFileNamePatterns, new SupportPackageOptions
            {
                FileSystemPaths = customData.ToArray(),
                DescriptionBuilder = builder
            });

            return result;
        }



        #endregion
    }
}
