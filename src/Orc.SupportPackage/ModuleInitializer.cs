using System.Runtime.CompilerServices;
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
    [ModuleInitializer]
    public static void Initialize()
    {
        var serviceLocator = ServiceLocator.Default;

        serviceLocator.RegisterType<ISupportPackageService, SupportPackageService>();
        serviceLocator.RegisterType<IScreenCaptureService, ScreenCaptureService>();

        var languageService = serviceLocator.ResolveRequiredType<ILanguageService>();
        languageService.RegisterLanguageSource(new LanguageResourceSource("Orc.SupportPackage", "Orc.SupportPackage.Properties", "Resources"));
    }
}
