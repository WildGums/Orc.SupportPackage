// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageService.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage.Services
{
    using System;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Xml.Serialization;
    using SystemInfo.Services;
    using Catel;
    using Catel.Logging;
    using Ionic.Zip;

    internal class SupportPackageService : ISupportPackageService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly IScreenCaptureService _screenCaptureService;
        private readonly ISystemInfoService _systemInfoService;

        public SupportPackageService(ISystemInfoService systemInfoService, IScreenCaptureService screenCaptureService)
        {
            Argument.IsNotNull(() => systemInfoService);
            Argument.IsNotNull(() => screenCaptureService);

            _systemInfoService = systemInfoService;
            _screenCaptureService = screenCaptureService;
        }

        public async Task<bool> CreateSupportPackage(string zipFileName)
        {
            Argument.IsNotNullOrEmpty(() => zipFileName);

            bool result = true;
            try
            {
                Log.Info("Creating support package");

                var applicationDataDirectory = Catel.IO.Path.GetApplicationDataDirectory();

                var sysinfoFileName = Path.Combine(applicationDataDirectory, "sysinfo.xml");
                GetAndSaveSystemInformation(sysinfoFileName);

                var screenshotFileName = Path.Combine(applicationDataDirectory, "screenshot.jpg");
                await CaptureWindowAndSave(screenshotFileName);

                using (var zipFile = new ZipFile())
                {
                    zipFile.AddDirectory(applicationDataDirectory, null);
                    zipFile.Save(zipFileName);
                }

                File.Delete(sysinfoFileName);
                File.Delete(screenshotFileName);

                Log.Info("Support package created");
            }
            catch (Exception ex)
            {
                result = false;
                Log.ErrorWithData(ex, "Error while creating support package");
            }

            return result;
        }

        private async Task CaptureWindowAndSave(string screenshotFile)
        {
            Argument.IsNotNullOrEmpty(() => screenshotFile);

            var mainWindow = Application.Current.MainWindow;
            var image = await _screenCaptureService.CaptureWindowImage(mainWindow);
            image.Save(screenshotFile, ImageFormat.Jpeg);
        }

        private void GetAndSaveSystemInformation(string sysInfoFile)
        {
            Argument.IsNotNullOrEmpty(() => sysInfoFile);

            var systemInfo = _systemInfoService.GetSystemInfo().AsParallel().ToList();

            var serilizer = new XmlSerializer(systemInfo.GetType());

            using (var fileStream = new FileStream(sysInfoFile, FileMode.OpenOrCreate))
            {
                serilizer.Serialize(fileStream, systemInfo);
            }
        }
    }
}