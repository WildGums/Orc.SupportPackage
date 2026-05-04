namespace Orc.SupportPackage;

using Catel;
using Catel.Fody;

public class SupportPackageDirectory : SupportPackageFileSystemArtifact
{
    public SupportPackageDirectory(string title, string directoryName, bool includeInSupportPackage = true)
        : base(title, includeInSupportPackage)
    {
        Argument.IsNotNullOrWhitespace(() => directoryName);

        DirectoryName = directoryName;
    }

    [NoWeaving]
    public string DirectoryName { get; }
}
