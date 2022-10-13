namespace Orc.SupportPackage
{
    using System.Threading.Tasks;

    public interface ISupportPackageService
    {
        Task<bool> CreateSupportPackageAsync(string zipFileName, string[] directories, string[] excludeFileNamePatterns);
    }
}
