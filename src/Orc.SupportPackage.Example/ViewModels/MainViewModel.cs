﻿// --------------------------------------------------------------------------------------------------------------------
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

    using Orc.SupportPackage.Services;
    using Orc.SystemInfo.Services;

    public class MainViewModel : ViewModelBase
    {
        // without this field picture doesn't reftesh
        private static int _screenshotIndex;

        private readonly IScreenCaptureService _screenCaptureService;

        private readonly ISystemInfoService _systemInfoService;

        public MainViewModel(IServiceLocator serviceLocator, IScreenCaptureService screenCaptureService, ISystemInfoService systemInfoService)
        {
            Argument.IsNotNull(() => serviceLocator);
            Argument.IsNotNull(() => screenCaptureService);
            Argument.IsNotNull(() => systemInfoService);

            _screenCaptureService = screenCaptureService;
            _systemInfoService = systemInfoService;

            Screenshot = new TaskCommand(OnScreenshotExecute);
            ShowSystemInfo = new TaskCommand(OnShowSystemInfoExecute);
        }

        #region Commands
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
            var sysInfoLines = _systemInfoService.GetSystemInfo().Select(x => string.Format("{0} {1}", x.Key, x.Value));
            SystemInfo = String.Join("\n", sysInfoLines);
        }
        #endregion

        #region Properties
        public BitmapImage ScreenPic { get; private set; }

        public string SystemInfo { get; set; }
        #endregion
    }
}