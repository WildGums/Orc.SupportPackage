namespace Orc.SupportPackage;

using System.Collections.Generic;

public interface ISupportPackageContentProvider
{
    IReadOnlyList<SupportPackageFileSystemArtifact> GetSupportPackageFileSystemArtifacts();
}
