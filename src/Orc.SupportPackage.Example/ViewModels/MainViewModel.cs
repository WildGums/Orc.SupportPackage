namespace Orc.SupportPackage.Example.ViewModels
{
    using System;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using Catel;
    using Catel.IoC;
    using Catel.MVVM;
    using Catel.Services;
    using Catel.Threading;
    using Orc.FileSystem;
    using Orc.SupportPackage.ViewModels;
    using SystemInfo;

    public class MainViewModel : ViewModelBase
    {
        // without this field picture doesn't refresh
        private static int ScreenshotIndex;

        private readonly IScreenCaptureService _screenCaptureService;
        private readonly ISystemInfoService _systemInfoService;
        private readonly IMessageService _messageService;
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IAppDataService _appDataService;
        private readonly ITypeFactory _typeFactory;
        private readonly IEncryptionService _encryptionService;
        private readonly IOpenFileService _openFileService;
        private readonly IFileService _fileService;

        public MainViewModel(IScreenCaptureService screenCaptureService, ISystemInfoService systemInfoService, IMessageService messageService,
            IUIVisualizerService uiVisualizerService, IAppDataService appDataService, ITypeFactory typeFactory, IEncryptionService encryptionService,
            IOpenFileService openFileService, IFileService fileService)
        {
            Argument.IsNotNull(() => screenCaptureService);
            Argument.IsNotNull(() => systemInfoService);
            Argument.IsNotNull(() => messageService);
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => appDataService);
            Argument.IsNotNull(() => typeFactory);
            Argument.IsNotNull(() => encryptionService);
            Argument.IsNotNull(() => openFileService);
            Argument.IsNotNull(() => fileService);

            _screenCaptureService = screenCaptureService;
            _systemInfoService = systemInfoService;
            _messageService = messageService;
            _uiVisualizerService = uiVisualizerService;
            _appDataService = appDataService;
            _typeFactory = typeFactory;
            _encryptionService = encryptionService;
            _openFileService = openFileService;
            _fileService = fileService;
            Screenshot = new TaskCommand(OnScreenshotExecuteAsync);
            ShowSystemInfo = new TaskCommand(OnShowSystemInfoExecuteAsync);
            SavePackage = new TaskCommand(OnSavePackageExecuteAsync);
            EncryptAndSavePackage = new TaskCommand(OnEncryptAndSavePackageExecuteAsync);
            GenerateKeys = new TaskCommand(OnGenerateKeysExecuteAsync);
            DecryptPackage = new TaskCommand(OnDecryptPackageExecuteAsync);

            Title = "Orc.SupportPackage example";

            var currentDirectory = Environment.CurrentDirectory;
            PublicKeyPath = Path.Combine(currentDirectory, "public.pem");
            PrivateKeyPath = Path.Combine(currentDirectory, "private.pem");
        }

        #region Commands
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

            if (!result.Result)
            {
                return;
            }

            var directory = Path.GetDirectoryName(result.FileName);
            var fileName = Path.GetFileNameWithoutExtension(result.FileName);

            var decryptedPackagePath = Path.Combine(directory, $"{fileName}_dec.spkg");

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
            var sysInfoElements = await TaskHelper.Run(() => _systemInfoService.GetSystemInfo(), true);
            var sysInfoLines = sysInfoElements.Select(x => x.ToString());
            SystemInfo = string.Join("\n", sysInfoLines);
        }

        #endregion

        #region Properties
        public BitmapImage ScreenPic { get; private set; }

        public string SystemInfo { get; set; }

        public string PrivateKeyPath { get; set; }

        public string PublicKeyPath { get; set; }
        #endregion
    }
}
