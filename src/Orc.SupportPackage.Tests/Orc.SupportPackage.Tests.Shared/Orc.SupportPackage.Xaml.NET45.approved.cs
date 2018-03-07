[assembly: System.Resources.NeutralResourcesLanguageAttribute("en-US")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("Orc.SupportPackage.Tests")]
[assembly: System.Runtime.InteropServices.ComVisibleAttribute(false)]
[assembly: System.Runtime.Versioning.TargetFrameworkAttribute(".NETFramework,Version=v4.5", FrameworkDisplayName=".NET Framework 4.5")]


public class static LoadAssembliesOnStartup { }
public class static ModuleInitializer
{
    public static void Initialize() { }
}
namespace Orc.SupportPackage
{
    
    public class CustomPathsPackageFileSystemArtifact : Orc.SupportPackage.SupportPackageFileSystemArtifact
    {
        public CustomPathsPackageFileSystemArtifact(string title, System.Collections.Generic.List<string> paths, bool includeInSupportPackage) { }
        public System.Collections.ObjectModel.ReadOnlyCollection<string> Paths { get; }
    }
    public sealed class DefaultSupportPackageContentProvider : Orc.SupportPackage.ISupportPackageContentProvider
    {
        public DefaultSupportPackageContentProvider(Catel.Services.ILanguageService languageService) { }
        [System.Runtime.CompilerServices.IteratorStateMachineAttribute(typeof(Orc.SupportPackage.DefaultSupportPackageContentProvider.<GetSupportPackageFileSystemArtifacts>d__2))]
        public System.Collections.Generic.IEnumerable<Orc.SupportPackage.SupportPackageFileSystemArtifact> GetSupportPackageFileSystemArtifacts() { }
    }
    public interface ISupportPackageBuilderService
    {
        System.Threading.Tasks.Task<bool> CreateSupportPackageAsync(string fileName, System.Collections.Generic.List<Orc.SupportPackage.SupportPackageFileSystemArtifact> artifacts);
    }
    public interface ISupportPackageContentProvider
    {
        System.Collections.Generic.IEnumerable<Orc.SupportPackage.SupportPackageFileSystemArtifact> GetSupportPackageFileSystemArtifacts();
    }
    public class SupportPackageBuilderService : Orc.SupportPackage.ISupportPackageBuilderService
    {
        public SupportPackageBuilderService(Orc.SupportPackage.ISupportPackageService supportPackageService, Orc.FileSystem.IFileService fileService) { }
        public System.Threading.Tasks.Task<bool> CreateSupportPackageAsync(string fileName, System.Collections.Generic.List<Orc.SupportPackage.SupportPackageFileSystemArtifact> artifacts) { }
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
    public class SupportPackageFileSystemArtifact
    {
        protected SupportPackageFileSystemArtifact(string title, bool includeInSupportPackage) { }
        public bool IncludeInSupportPackage { get; set; }
        public string Title { get; set; }
    }
}
namespace Orc.SupportPackage.ViewModels
{
    
    public class SupportPackageViewModel : Catel.MVVM.ViewModelBase
    {
        public static readonly Catel.Data.PropertyData IncludeCustomPathsInSupportPackageProperty;
        public static readonly Catel.Data.PropertyData LastSupportPackageFileNameProperty;
        public SupportPackageViewModel(Catel.Services.ISaveFileService saveFileService, Orc.SupportPackage.ISupportPackageBuilderService supportPackageService, Catel.Services.IPleaseWaitService pleaseWaitService, Catel.Services.IProcessService processService, Catel.Services.ISelectDirectoryService selectDirectoryService, Catel.Services.IOpenFileService openFileService, Catel.Services.ILanguageService languageService, Catel.IoC.IServiceLocator serviceLocator) { }
        public Catel.MVVM.TaskCommand AddDirectoryCommand { get; set; }
        public Catel.MVVM.TaskCommand AddFileCommand { get; }
        public Catel.MVVM.TaskCommand CreateSupportPackage { get; }
        public Catel.Collections.FastObservableCollection<string> CustomPaths { get; }
        public bool IncludeCustomPathsInSupportPackage { get; set; }
        public string LastSupportPackageFileName { get; }
        public Catel.MVVM.Command OpenDirectory { get; }
        public Catel.MVVM.Command RemovePathCommand { get; }
        public System.Collections.Generic.List<string> SelectedCustomPaths { get; }
        public Catel.MVVM.Command<System.Windows.Controls.SelectionChangedEventArgs> SelectionChangedCommand { get; }
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