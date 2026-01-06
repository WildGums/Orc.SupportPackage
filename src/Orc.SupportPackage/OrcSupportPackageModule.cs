namespace Orc.SupportPackage
{
    using Catel.Services;
    using Catel.ThirdPartyNotices;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Core module which allows the registration of default services in the service collection.
    /// </summary>
    public static class OrcSupportPackageModule
    {
        public static IServiceCollection AddOrcSupportPackage(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<ISupportPackageService, SupportPackageService>();
            serviceCollection.TryAddSingleton<IScreenCaptureService, ScreenCaptureService>();

            serviceCollection.AddSingleton<ILanguageSource>(new LanguageResourceSource("Orc.SupportPackage", "Orc.SupportPackage.Properties", "Resources"));

            serviceCollection.AddSingleton<IThirdPartyNotice>((x) => new LibraryThirdPartyNotice("Orc.SupportPackage", "https://github.com/wildgums/orc.supportpackage"));

            return serviceCollection;
        }
    }
}
