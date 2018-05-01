[assembly: System.Resources.NeutralResourcesLanguageAttribute("en-US")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("Orc.SupportPackage.Tests")]
[assembly: System.Runtime.Versioning.TargetFrameworkAttribute(".NETFramework,Version=v4.6", FrameworkDisplayName=".NET Framework 4.6")]
public class static LoadAssembliesOnStartup { }
public class static ModuleInitializer
{
    public static void Initialize() { }
}
namespace Orc.SupportPackage
{
    public interface IScreenCaptureService
    {
        System.Drawing.Image CaptureWindowImage(System.Windows.Window window);
    }
    public interface ISupportPackageContext
    {
        string RootDirectory { get; }
        string GetDirectory(string relativeDirectoryName);
        string GetFile(string relativeFilePath);
    }
    public interface ISupportPackageProvider
    {
        System.Threading.Tasks.Task ProvideAsync(Orc.SupportPackage.ISupportPackageContext supportPackageContext);
    }
    public interface ISupportPackageService
    {
        System.Threading.Tasks.Task<bool> CreateSupportPackageAsync(string zipFileName, string[] directories, string[] excludeFileNamePatterns);
    }
    public struct Rect
    {
        public int bottom;
        public int left;
        public int right;
        public int top;
    }
    public class ScreenCaptureService : Orc.SupportPackage.IScreenCaptureService
    {
        public ScreenCaptureService() { }
        public System.Drawing.Image CaptureWindowImage(System.Windows.Window window) { }
    }
    public class SupportPackageContext : Orc.SupportPackage.ISupportPackageContext, System.IDisposable
    {
        public SupportPackageContext() { }
        public string RootDirectory { get; }
        public void Dispose() { }
        public string GetDirectory(string relativeDirectoryName) { }
        public string GetFile(string relativeFilePath) { }
    }
    public abstract class SupportPackageProviderBase : Orc.SupportPackage.ISupportPackageProvider
    {
        protected SupportPackageProviderBase() { }
        public abstract System.Threading.Tasks.Task ProvideAsync(Orc.SupportPackage.ISupportPackageContext supportPackageContext);
    }
    public class SupportPackageService : Orc.SupportPackage.ISupportPackageService
    {
        public SupportPackageService(Orc.SystemInfo.ISystemInfoService systemInfoService, Orc.SupportPackage.IScreenCaptureService screenCaptureService, Catel.IoC.ITypeFactory typeFactory, Orc.FileSystem.IDirectoryService directoryService) { }
        public System.Threading.Tasks.Task<bool> CreateSupportPackageAsync(string zipFileName, string[] directories, string[] excludeFileNamePatterns) { }
    }
}