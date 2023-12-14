namespace Orc.SupportPackage;

using System.Threading.Tasks;

public abstract class SupportPackageProviderBase : ISupportPackageProvider
{
    public abstract Task ProvideAsync(ISupportPackageContext supportPackageContext);
}
