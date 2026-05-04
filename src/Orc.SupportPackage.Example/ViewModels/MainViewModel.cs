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
    private readonly IEncryptionService _encryptionService;
    private readonly ITypeFactory _typeFactory;
    private readonly IOpenFileService _openFileService;
    private readonly IMessageService _messageService;

    public MainViewModel(IScreenCaptureService screenCaptureService, ISystemInfoService systemInfoService,
        IUIVisualizerService uiVisualizerService, IAppDataService appDataService,
        IDirectoryService directoryService, IFileService fileService, IServiceProvider serviceProvider,
        IEncryptionService encryptionService, ITypeFactory typeFactory, IOpenFileService openFileService,
        IMessageService messageService)
        : base(serviceProvider)
    {
        _screenCaptureService = screenCaptureService;
        _systemInfoService = systemInfoService;
        _uiVisualizerService = uiVisualizerService;
        _appDataService = appDataService;
        _directoryService = directoryService;
        _fileService = fileService;
        _encryptionService = encryptionService;
        _typeFactory = typeFactory;
        _openFileService = openFileService;
        _messageService = messageService;

        Screenshot = new TaskCommand(serviceProvider, OnScreenshotExecuteAsync);
        ShowSystemInfo = new TaskCommand(serviceProvider, OnShowSystemInfoExecuteAsync);
        SavePackage = new TaskCommand(serviceProvider, OnSavePackageExecuteAsync);
        EncryptAndSavePackage = new TaskCommand(serviceProvider, OnEncryptAndSavePackageExecuteAsync);
        GenerateKeys = new TaskCommand(serviceProvider, OnGenerateKeysExecuteAsync);
        DecryptPackage = new TaskCommand(serviceProvider, OnDecryptPackageExecuteAsync);

        Title = "Orc.SupportPackage example";

        var currentDirectory = Environment.CurrentDirectory;
        PublicKeyPath = Path.Combine(currentDirectory, "public.pem");
        PrivateKeyPath = Path.Combine(currentDirectory, "private.pem");
    }

    public TaskCommand SavePackage { get; private set; }

    private async Task OnSavePackageExecuteAsync()
    {
        await _uiVisualizerService.ShowDialogAsync<SupportPackageViewModel>();
    }

    public TaskCommand EncryptAndSavePackage { get; private set; }

    private async Task OnEncryptAndSavePackageExecuteAsync()
    {
        var supportPackageViewModel = _typeFactory.CreateInstance<SupportPackageViewModel>();
        supportPackageViewModel.EncryptionContext = new EncryptionContext
        {
            PrivateKeyPath = PrivateKeyPath,
            PublicKey = await _encryptionService.ReadPublicKeyFromPemFileAsync(PublicKeyPath)
        };

        await _uiVisualizerService.ShowDialogAsync(supportPackageViewModel);
    }

    public TaskCommand GenerateKeys { get; private set; }

    private async Task OnGenerateKeysExecuteAsync()
    {
        _encryptionService.Generate(PrivateKeyPath, PublicKeyPath);
        await _messageService.ShowInformationAsync("Encryption keys generated");
    }

    public TaskCommand DecryptPackage { get; private set; }

    private async Task OnDecryptPackageExecuteAsync()
    {
        var result = await _openFileService.DetermineFileAsync(new DetermineOpenFileContext
        {
        });

        if (!result.Result || result.FileName is null)
        {
            return;
        }

        var directory = Path.GetDirectoryName(result.FileName);
        var fileName = Path.GetFileNameWithoutExtension(result.FileName);

        var decryptedPackagePath = Path.Combine(directory ?? string.Empty, $"{fileName}_dec.spkg");

        using (var sourceStream = _fileService.OpenRead(result.FileName))
        {
            if (_fileService.Exists(decryptedPackagePath))
            {
                _fileService.Delete(decryptedPackagePath);
            }

            using (var targetStream = _fileService.Create(decryptedPackagePath))
            {
                await _encryptionService.DecryptAsync(sourceStream, targetStream, new EncryptionContext
                {
                    PrivateKeyPath = PrivateKeyPath,
                    PublicKey = await _encryptionService.ReadPublicKeyFromPemFileAsync(PublicKeyPath)
                });
            }
        }

        await _messageService.ShowInformationAsync($"Decrypted support package saved on path {decryptedPackagePath}");
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

    public string PrivateKeyPath { get; set; }

    public string PublicKeyPath { get; set; }
}
