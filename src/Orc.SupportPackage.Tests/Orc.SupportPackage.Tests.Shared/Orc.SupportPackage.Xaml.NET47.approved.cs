[assembly: System.Resources.NeutralResourcesLanguageAttribute("en-US")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("Orc.SupportPackage.Tests")]
[assembly: System.Runtime.InteropServices.ComVisibleAttribute(false)]
[assembly: System.Runtime.Versioning.TargetFrameworkAttribute(".NETFramework,Version=v4.7", FrameworkDisplayName=".NET Framework 4.7")]


public class static LoadAssembliesOnStartup { }
public class static ModuleInitializer
{
    public static void Initialize() { }
}
namespace Orc.SupportPackage
{
    
    public sealed class DefaultSupportPackageContentProvider : Orc.SupportPackage.ISupportPackageContentProvider
    {
        public DefaultSupportPackageContentProvider(Catel.Services.ILanguageService languageService) { }
        [System.Runtime.CompilerServices.IteratorStateMachineAttribute(typeof(Orc.SupportPackage.DefaultSupportPackageContentProvider.<GetSupportPackageFileSystemArtifacts>d__2))]
        public System.Collections.Generic.IEnumerable<Orc.SupportPackage.SupportPackageFileSystemArtifact> GetSupportPackageFileSystemArtifacts() { }
    }
    public interface ISupportPackageContentProvider
    {
        System.Collections.Generic.IEnumerable<Orc.SupportPackage.SupportPackageFileSystemArtifact> GetSupportPackageFileSystemArtifacts();
    }
    public class SupportPackageDirectory : Orc.SupportPackage.SupportPackageFileSystemArtifact
    {
        public SupportPackageDirectory(string title, string directoryName, bool includeInSupportPackage = True) { }
        public string DirectoryName { get; }
    }
    public class SupportPackageFileNamePattern : Orc.SupportPackage.SupportPackageFileSystemArtifact
    {
        public SupportPackageFileNamePattern(string title, string[] fileNamePatterns, bool includeInSupportPackage = True) { }
        public string[] FileNamePatterns { get; }
    }
    public class SupportPackageFileSystemArtifact : Catel.Data.ModelBase
    {
        public static readonly Catel.Data.PropertyData IncludeInSupportPackageProperty;
        public static readonly Catel.Data.PropertyData TitleProperty;
        protected SupportPackageFileSystemArtifact(string title, bool includeInSupportPackage) { }
        public bool IncludeInSupportPackage { get; set; }
        public string Title { get; set; }
    }
}
namespace Orc.SupportPackage.ViewModels
{
    
    public class SupportPackageViewModel : Catel.MVVM.ViewModelBase
    {
        public static readonly Catel.Data.PropertyData LastSupportPackageFileNameProperty;
        public SupportPackageViewModel(Catel.Services.ISaveFileService saveFileService, Orc.SupportPackage.ISupportPackageService supportPackageService, Catel.Services.IPleaseWaitService pleaseWaitService, Catel.Services.IProcessService processService, Catel.Services.ILanguageService languageService, Catel.IoC.IServiceLocator serviceLocator) { }
        public Catel.MVVM.TaskCommand CreateSupportPackage { get; }
        public string LastSupportPackageFileName { get; }
        public Catel.MVVM.Command OpenDirectory { get; }
        public System.Collections.Generic.List<Orc.SupportPackage.SupportPackageFileSystemArtifact> SupportPackageFileSystemArtifacts { get; }
    }
}
namespace Orc.SupportPackage.Views
{
    
    public class SupportPackageWindow : Catel.Windows.DataWindow, System.Windows.Markup.IComponentConnector
    {
        public SupportPackageWindow() { }
        public SupportPackageWindow(Orc.SupportPackage.ViewModels.SupportPackageViewModel viewModel) { }
        public void InitializeComponent() { }
    }
}