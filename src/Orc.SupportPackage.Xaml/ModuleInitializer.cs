using Catel.IoC;
using Catel.Services;
using Orc.SupportPackage;

/// <summary>
/// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
/// </summary>
public static class ModuleInitializer
{
    /// <summary>
    /// Initializes the module.
    /// </summary>
    public static void Initialize()
    {
        var serviceLocator = ServiceLocator.Default;

        serviceLocator.RegisterType<ISupportPackageBuilderService, SupportPackageBuilderService>();
        serviceLocator.RegisterType<ISupportPackageContentProvider, DefaultSupportPackageContentProvider>();

        var languageService = serviceLocator.ResolveRequiredType<ILanguageService>();
        languageService.RegisterLanguageSource(new LanguageResourceSource("Orc.SupportPackage.Xaml", "Orc.SupportPackage.Properties", "Resources"));
    }
}
