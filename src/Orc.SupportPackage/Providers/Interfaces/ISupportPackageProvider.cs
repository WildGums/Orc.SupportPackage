namespace Orc.SupportPackage
{
    using System.Threading.Tasks;

    public interface ISupportPackageProvider
    {
        Task ProvideAsync(ISupportPackageContext supportPackageContext);
    }
}
