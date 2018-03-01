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

    using Ionic.Zip;

    public class SupportPackageBuilderService : ISupportPackageBuilderService
    {
        #region Fields
        private readonly ISupportPackageService _supportPackageService;
        #endregion

        #region Constructors
        public SupportPackageBuilderService(ISupportPackageService supportPackageService)
        {
            Argument.IsNotNull(() => supportPackageService);

            _supportPackageService = supportPackageService;
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

            builder.AppendLine();
            builder.AppendLine("## File system entries");
            builder.AppendLine();
            using (var zipFile = new ZipFile(fileName))
            {
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