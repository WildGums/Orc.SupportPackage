namespace Orc.SupportPackage
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using Catel;

    /// <summary>
    /// The custom paths package file system artifact.
    /// </summary>
    public class CustomPathsPackageFileSystemArtifact : SupportPackageFileSystemArtifact
    {
        public CustomPathsPackageFileSystemArtifact(string title, List<string> paths, bool includeInSupportPackage)
            : base(title, includeInSupportPackage)
        {
            Argument.IsNotNullOrWhitespace(() => title);

            Title = title;
            Paths = paths.AsReadOnly();
            IncludeInSupportPackage = includeInSupportPackage;
        }

        public ReadOnlyCollection<string> Paths { get; }
    }
}
