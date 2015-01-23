namespace Orc.SupportPackage.Services
{
    using System.Threading.Tasks;

    public interface ISupportPackageService
    {
        Task CreateSupportPackage(string zipFileName);
    }
}