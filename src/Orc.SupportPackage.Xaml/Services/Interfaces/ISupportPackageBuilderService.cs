namespace Orc.SupportPackage
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISupportPackageBuilderService
    {
        Task<bool> CreateSupportPackageAsync(string fileName, List<SupportPackageFileSystemArtifact> artifacts);
    }
}
