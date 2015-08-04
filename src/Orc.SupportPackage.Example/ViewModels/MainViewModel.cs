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
    using SystemInfo;
    using Catel;
    using Catel.IoC;
    using Catel.MVVM;
    using Catel.Services;
    using Catel.Threading;
    using SupportPackage.ViewModels;

    public class MainViewModel : ViewModelBase
    {
        // without this field picture doesn't refresh
        private static int _screenshotIndex;

        private readonly IScreenCaptureService _screenCaptureService;

        private readonly ISystemInfoService _systemInfoService;

        private readonly IUIVisualizerService _uiVisualizerService;

        public MainViewModel(IScreenCaptureService screenCaptureService, ISystemInfoService systemInfoService, IUIVisualizerService uiVisualizerService)
        {
            Argument.IsNotNull(() => screenCaptureService);
            Argument.IsNotNull(() => systemInfoService);
            Argument.IsNotNull(() => uiVisualizerService);

            _screenCaptureService = screenCaptureService;
            _systemInfoService = systemInfoService;
            _uiVisualizerService = uiVisualizerService;

            Screenshot = new TaskCommand(OnScreenshotExecuteAsync);
            ShowSystemInfo = new TaskCommand(OnShowSystemInfoExecuteAsync);
            SavePackage = new Command(OnSavePackageExecute);
        }

        #region Commands
        public Command SavePackage { get; private set; }

        private void OnSavePackageExecute()
        {
            _uiVisualizerService.ShowDialog<SupportPackageViewModel>();
        }

        public TaskCommand Screenshot { get; private set; }

        private async Task OnScreenshotExecuteAsync()
        {
            ScreenPic = null;

            var mainWindow = Application.Current.MainWindow;
            var image = await TaskHelper.Run(() => _screenCaptureService.CaptureWindowImage(mainWindow));
            var applicationDataDirectory = Catel.IO.Path.GetApplicationDataDirectory();
            var filename = Path.Combine(applicationDataDirectory, string.Format("screenshot{0}.jpg", _screenshotIndex++));
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
            var sysInfoElements = await TaskHelper.Run(() => _systemInfoService.GetSystemInfo());
            var sysInfoLines = sysInfoElements.Select(x => x.ToString());
            SystemInfo = String.Join("\n", sysInfoLines);
        }
        #endregion

        #region Properties
        public BitmapImage ScreenPic { get; private set; }

        public string SystemInfo { get; set; }
        #endregion
    }
}