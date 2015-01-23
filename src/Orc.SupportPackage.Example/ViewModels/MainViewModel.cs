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
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media.Imaging;

    using Catel.IoC;
    using Catel.MVVM;

    using Orc.SupportPackage.Services;

    public class MainViewModel : ViewModelBase
    {
        // without this field picture doesn't reftesh
        private static int _screenshotIndex = 0;

        private readonly IScreenCaptureService _screenCaptureService;

        public MainViewModel(IServiceLocator serviceLocator)
        {
            _screenCaptureService = serviceLocator.ResolveType<IScreenCaptureService>();
            Screenshot = new TaskCommand(OnScreenshotExecute);
        }

        public TaskCommand Screenshot { get; private set; }

        public BitmapImage ScreenPic { get; set; }

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
    }
}