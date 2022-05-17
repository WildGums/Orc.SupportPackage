// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageBuilderService.cs" company="WildGums">
//   Copyright (c) 2008 - 2018 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System.Collections.Generic;
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
            return await CreateSupportPackageAsync(new SupportPackageBuilderContext
            {
                FileName = fileName,
                Artifacts = artifacts,
            });
        }

        public async Task<bool> CreateSupportPackageAsync(SupportPackageBuilderContext context)
        {
            var fileName = context.FileName;
            var artifacts = context.Artifacts;

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

            using (var supportPackageContext = new SupportPackageContext())
            {
                supportPackageContext.ZipFileName = fileName;
                supportPackageContext.AddArtifactDirectories(directories);
                supportPackageContext.AddExcludeFileNamePatterns(excludeFileNamePatterns);
                supportPackageContext.AddCustomFileSystemPaths(customData.ToArray());
                supportPackageContext.DescriptionBuilder = builder;
                supportPackageContext.IsEncrypted = context.IsEncrypted;
                supportPackageContext.EncryptionContext = context.EncryptionContext;

                var result = await _supportPackageService.CreateSupportPackageAsync(supportPackageContext);

                return result;
            }
        }

        #endregion
    }
}
