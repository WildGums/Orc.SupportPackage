// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageService.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage.Services
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Xml.Serialization;
    using SystemInfo.Models;
    using Catel;
    using Catel.Logging;
    using Ionic.Zip;

    using Orc.SystemInfo.Services;

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

        public async Task CreateSupportPackage(string zipFileName)
        {
            Argument.IsNotNullOrEmpty(() => zipFileName);

            try
            {
                var applicationDataDirectory = Catel.IO.Path.GetApplicationDataDirectory();

                var sysinfoFileName = Path.Combine(applicationDataDirectory, "sysinfo.xml");
                SaveSysInfo(sysinfoFileName);

                var screenshotFileName = Path.Combine(applicationDataDirectory, "screenshot.jpg");
                await SaveScreenshot(screenshotFileName);

                using (var zipFile = new ZipFile())
                {
                    zipFile.AddDirectory(applicationDataDirectory, null);
                    zipFile.Save(zipFileName);
                }

                File.Delete(sysinfoFileName);
                File.Delete(screenshotFileName);
            }
            catch (Exception ex)
            {
                Log.ErrorWithData(ex, "Error while createin support package");
            }
        }

        private async Task SaveScreenshot(string screenshotFile)
        {
            Argument.IsNotNullOrEmpty(() => screenshotFile);

            var mainWindow = Application.Current.MainWindow;
            var image = await _screenCaptureService.CaptureWindowImage(mainWindow);
            image.Save(screenshotFile, ImageFormat.Jpeg);
        }

        private void SaveSysInfo(string sysInfoFile)
        {
            Argument.IsNotNullOrEmpty(() => sysInfoFile);

            var systemInfo = _systemInfoService.GetSystemInfo().AsParallel().ToList();
            SaveSystemInfo(sysInfoFile, systemInfo);
        }

        private static void SaveSystemInfo(string fileName, IEnumerable<CoupledValue<string, string>> sysInfo)
        {
            Argument.IsNotNullOrEmpty(() => fileName);
            Argument.IsNotNull(() => sysInfo);

            var serilizer = new XmlSerializer(sysInfo.GetType());

            using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                serilizer.Serialize(fileStream, sysInfo);
            }
        }
    }
}