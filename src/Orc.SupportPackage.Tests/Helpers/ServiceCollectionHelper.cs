namespace Orc.SupportPackage.Tests;

using Catel;
using Microsoft.Extensions.DependencyInjection;
using Orc.FileSystem;
using Orc.SupportPackage;
using Orc.SystemInfo;

internal static class ServiceCollectionHelper
{
    public static IServiceCollection CreateServiceCollection()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddLogging();
        serviceCollection.AddCatelCore();
        serviceCollection.AddCatelMvvm();
        serviceCollection.AddOrcFileSystem();
        serviceCollection.AddOrcSystemInfo();
        serviceCollection.AddOrcSupportPackage();
        serviceCollection.AddOrcSupportPackageXaml();

        return serviceCollection;
    }
}
