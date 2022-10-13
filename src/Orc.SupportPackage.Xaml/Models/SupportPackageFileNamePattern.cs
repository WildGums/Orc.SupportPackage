namespace Orc.SupportPackage
{
    using System.Linq;

    using Catel;
    using Catel.Fody;

    public class SupportPackageFileNamePattern : SupportPackageFileSystemArtifact
    {
        public SupportPackageFileNamePattern(string title, string[] fileNamePatterns, bool includeInSupportPackage = true)
            : base((title + " " + fileNamePatterns.Aggregate("(", (current, fileExtension) => current + fileExtension + " | ").TrimEnd('|', ' ') + ")").Trim(), includeInSupportPackage)
        {
            Argument.IsNotNullOrEmptyArray(() => fileNamePatterns);

            FileNamePatterns = fileNamePatterns;
        }

        [NoWeaving]
        public string[] FileNamePatterns { get; }
    }
}
