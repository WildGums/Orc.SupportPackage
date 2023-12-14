namespace Orc.SupportPackage;

using Catel;

public class SupportPackageFileSystemArtifact
{
    protected SupportPackageFileSystemArtifact(string title, bool includeInSupportPackage)
    {
        Argument.IsNotNullOrWhitespace(() => title);

        Title = title;
        IncludeInSupportPackage = includeInSupportPackage;
    }

    public string Title { get; set; }

    public bool IncludeInSupportPackage { get; set; }
}
