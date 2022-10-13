namespace Orc.SupportPackage
{
    using System.Collections.Generic;

    public interface ISupportPackageContentProvider
    {
        IEnumerable<SupportPackageFileSystemArtifact> GetSupportPackageFileSystemArtifacts();
    }
}
