namespace Orc.SupportPackage.Example.ViewModels;

using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using SystemInfo;
using Catel.MVVM;
using Catel.Services;
using Orc.SupportPackage.ViewModels;
using Orc.FileSystem;

public class MainViewModel : ViewModelBase
{
    // without this field picture doesn't refresh
    private static int ScreenshotIndex;

    private readonly IScreenCaptureService _screenCaptureService;
    private readonly ISystemInfoService _systemInfoService;
    private readonly IUIVisualizerService _uiVisualizerService;
    private readonly IAppDataService _appDataService;
    private readonly IDirectoryService _directoryService;
    private readonly IFileService _fileService;

    public MainViewModel(IScreenCaptureService screenCaptureService, ISystemInfoService systemInfoService,
        IUIVisualizerService uiVisualizerService, IAppDataService appDataService,
        IDirectoryService directoryService, IFileService fileService, IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _screenCaptureService = screenCaptureService;
        _systemInfoService = systemInfoService;
        _uiVisualizerService = uiVisualizerService;
        _appDataService = appDataService;
        _directoryService = directoryService;
        _fileService = fileService;

        Screenshot = new TaskCommand(serviceProvider, OnScreenshotExecuteAsync);
        ShowSystemInfo = new TaskCommand(serviceProvider, OnShowSystemInfoExecuteAsync);
        SavePackage = new TaskCommand(serviceProvider, OnSavePackageExecuteAsync);

        Title = "Orc.SupportPackage example";
    }

    public TaskCommand SavePackage { get; private set; }

    private async Task OnSavePackageExecuteAsync()
    {
        await _uiVisualizerService.ShowDialogAsync<SupportPackageViewModel>();
    }

    public TaskCommand Screenshot { get; private set; }

    private async Task OnScreenshotExecuteAsync()
    {
        ScreenPic = null;

        var mainWindow = Application.Current.MainWindow;

#pragma warning disable IDISP001
        var image = _screenCaptureService.CaptureWindowImage(mainWindow);
#pragma warning restore IDISP001

        var applicationDataDirectory = _appDataService.GetApplicationDataDirectory(Catel.IO.ApplicationDataTarget.UserRoaming);

        _directoryService.Create(applicationDataDirectory);

        var filename = Path.Combine(applicationDataDirectory, string.Format("screenshot{0}.jpg", ScreenshotIndex++));
        image.Save(filename, ImageFormat.Jpeg);

        var screenPic = new BitmapImage();
        screenPic.BeginInit();
        screenPic.CacheOption = BitmapCacheOption.OnLoad;
        screenPic.UriSource = new Uri(filename);
        screenPic.EndInit();

        ScreenPic = screenPic;
    }

    public TaskCommand ShowSystemInfo { get; private set; }

    private async Task OnShowSystemInfoExecuteAsync()
    {
        var sysInfoElements = await Task.Run(() => _systemInfoService.GetSystemInfo());
        var sysInfoLines = sysInfoElements.Select(x => x.ToString());
        SystemInfo = string.Join("\n", sysInfoLines);
    }

    public BitmapImage ScreenPic { get; private set; }

    public string SystemInfo { get; set; }
}
