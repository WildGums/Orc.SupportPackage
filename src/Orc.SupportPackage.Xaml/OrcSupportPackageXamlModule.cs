namespace Orc;

using Catel.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orc.SupportPackage;

/// <summary>
/// Core module which allows the registration of default services in the service collection.
/// </summary>
public static class OrcSupportPackageXamlModule
{
    public static IServiceCollection AddOrcSupportPackageXaml(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<ISupportPackageBuilderService, SupportPackageBuilderService>();
        serviceCollection.TryAddSingleton<ISupportPackageContentProvider, DefaultSupportPackageContentProvider>();

        serviceCollection.AddSingleton<ILanguageSource>(new LanguageResourceSource("Orc.SupportPackage.Xaml", "Orc.SupportPackage.Properties", "Resources"));

        return serviceCollection;
    }
}
