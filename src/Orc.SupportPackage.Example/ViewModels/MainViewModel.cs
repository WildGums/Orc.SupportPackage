// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


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

    using Orc.SupportPackage.Services;
    using Orc.SystemInfo.Services;

    public class MainViewModel : ViewModelBase
    {
        // without this field picture doesn't reftesh
        private static int _screenshotIndex;

        private readonly IScreenCaptureService _screenCaptureService;

        private readonly ISystemInfoService _systemInfoService;

        private readonly ISaveFileService _saveFileService;

        private readonly ISupportPackageService _supportPackageService;

        public MainViewModel(IScreenCaptureService screenCaptureService, ISystemInfoService systemInfoService, ISaveFileService saveFileService, ISupportPackageService supportPackageService)
        {
            Argument.IsNotNull(() => screenCaptureService);
            Argument.IsNotNull(() => systemInfoService);
            Argument.IsNotNull(() => saveFileService);
            Argument.IsNotNull(() => supportPackageService);

            _screenCaptureService = screenCaptureService;
            _systemInfoService = systemInfoService;
            _saveFileService = saveFileService;
            _supportPackageService = supportPackageService;

            Screenshot = new TaskCommand(OnScreenshotExecute);
            ShowSystemInfo = new TaskCommand(OnShowSystemInfoExecute);
            SavePackage = new Command(OnSavePackageExecute);
        }        

        #region Commands
        public Command SavePackage { get; private set; }

        private void OnSavePackageExecute()
        {
            _saveFileService.Filter = "Zip files|*.zip";
            if (_saveFileService.DetermineFile())
            {
                _supportPackageService.CreateSupportPackage(_saveFileService.FileName);
            }
        }

        public TaskCommand Screenshot { get; private set; }

        private async Task OnScreenshotExecute()
        {
            ScreenPic = null;

            var mainWindow = Application.Current.MainWindow;
            var image = await _screenCaptureService.CaptureWindowImage(mainWindow);
            var applicationDataDirectory = Catel.IO.Path.GetApplicationDataDirectory();
            var filename = Path.Combine(applicationDataDirectory, string.Format("screenshot{0}.jpg", _screenshotIndex++));
            image.Save(filename, ImageFormat.Jpeg);

            ScreenPic = new BitmapImage();
            ScreenPic.BeginInit();
            ScreenPic.CacheOption = BitmapCacheOption.OnLoad;
            ScreenPic.UriSource = new Uri(filename);
            ScreenPic.EndInit();
        }

        public TaskCommand ShowSystemInfo { get; private set; }

        private async Task OnShowSystemInfoExecute()
        {
            var sysInfoLines = _systemInfoService.GetSystemInfo().Select(x => string.Format("{0} {1}", x.Value1, x.Value2));
            SystemInfo = String.Join("\n", sysInfoLines);
        }
        #endregion

        #region Properties
        public BitmapImage ScreenPic { get; private set; }

        public string SystemInfo { get; set; }
        #endregion
    }
}