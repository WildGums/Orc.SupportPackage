﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
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
    using Catel.MVVM;
    using Catel.Services;
    using Catel.Threading;
    using Orc.SupportPackage.ViewModels;

    public class MainViewModel : ViewModelBase
    {
        // without this field picture doesn't refresh
        private static int _screenshotIndex;

        private readonly IScreenCaptureService _screenCaptureService;
        private readonly ISystemInfoService _systemInfoService;
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IAppDataService _appDataService;

        public MainViewModel(IScreenCaptureService screenCaptureService, ISystemInfoService systemInfoService, 
            IUIVisualizerService uiVisualizerService, IAppDataService appDataService)
        {
            Argument.IsNotNull(() => screenCaptureService);
            Argument.IsNotNull(() => systemInfoService);
            Argument.IsNotNull(() => uiVisualizerService);
            Argument.IsNotNull(() => appDataService);

            _screenCaptureService = screenCaptureService;
            _systemInfoService = systemInfoService;
            _uiVisualizerService = uiVisualizerService;
            _appDataService = appDataService;

            Screenshot = new TaskCommand(OnScreenshotExecuteAsync);
            ShowSystemInfo = new TaskCommand(OnShowSystemInfoExecuteAsync);
            SavePackage = new TaskCommand(OnSavePackageExecuteAsync);

            Title = "Orc.SupportPackage example";
        }

        #region Commands
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
            var image = _screenCaptureService.CaptureWindowImage(mainWindow);
            var applicationDataDirectory = _appDataService.GetApplicationDataDirectory(Catel.IO.ApplicationDataTarget.UserRoaming);
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
            var sysInfoElements = await TaskHelper.Run(() => _systemInfoService.GetSystemInfo(), true);
            var sysInfoLines = sysInfoElements.Select(x => x.ToString());
            SystemInfo = string.Join("\n", sysInfoLines);
        }
        #endregion

        #region Properties
        public BitmapImage ScreenPic { get; private set; }

        public string SystemInfo { get; set; }
        #endregion
    }
}
