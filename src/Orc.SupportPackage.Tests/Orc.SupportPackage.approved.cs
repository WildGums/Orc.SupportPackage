﻿[assembly: System.Resources.NeutralResourcesLanguage("en-US")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Orc.SupportPackage.Tests")]
[assembly: System.Runtime.Versioning.TargetFramework(".NETCoreApp,Version=v5.0", FrameworkDisplayName="")]
public static class LoadAssembliesOnStartup { }
public static class ModuleInitializer
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
    public class ScreenCaptureService : Orc.SupportPackage.IScreenCaptureService
    {
        public ScreenCaptureService() { }
        public System.Drawing.Image CaptureWindowImage(System.Windows.Window window) { }
    }
    public class SupportPackageContext : Catel.Disposable, Orc.SupportPackage.ISupportPackageContext
    {
        public SupportPackageContext() { }
        public string RootDirectory { get; }
        protected override void DisposeManaged() { }
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
        public SupportPackageService(Orc.SystemInfo.ISystemInfoService systemInfoService, Orc.SupportPackage.IScreenCaptureService screenCaptureService, Catel.IoC.ITypeFactory typeFactory, Orc.FileSystem.IDirectoryService directoryService, Catel.Services.IAppDataService appDataService) { }
        public System.Threading.Tasks.Task<bool> CreateSupportPackageAsync(string zipFileName, string[] directories, string[] excludeFileNamePatterns) { }
    }
}