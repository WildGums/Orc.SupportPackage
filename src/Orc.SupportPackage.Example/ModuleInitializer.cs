using System.Runtime.CompilerServices;
using Catel.IoC;
using Catel.MVVM;

using Orc.SupportPackage.Example.Views;
using Orc.SupportPackage.Example.ViewModels;

using Orc.SupportPackage;
using Orc.SupportPackage.Example.SupportPackage.Providers;

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

        serviceLocator.RegisterTypeWithTag<ISupportPackageContentProvider, CustomSupportPackageContentProvider>("custom");

        var viewModelLocator = serviceLocator.ResolveRequiredType<IViewModelLocator>();
        viewModelLocator.Register(typeof(MainView), typeof(MainViewModel));
    }
}
