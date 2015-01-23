// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageService.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage.Services
{
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Xml.Serialization;

    using Catel;

    using Ionic.Zip;

    using Orc.SystemInfo.Services;

    internal class SupportPackageService : ISupportPackageService
    {
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

            var applicationDataDirectory = Catel.IO.Path.GetApplicationDataDirectory();

            var sysinfoFileName = Path.Combine(applicationDataDirectory, "sysinfo.xml");
            var sysInfoFile = SaveSysInfo(sysinfoFileName);

            var screenshotFileName = Path.Combine(applicationDataDirectory, "screenshot.jpg");
            var screenshotFile = await SaveScreenshot(screenshotFileName);

            using (var zipFile = new ZipFile())
            {
                zipFile.AddDirectory(applicationDataDirectory, null);
                zipFile.Save(zipFileName);
            }

            File.Delete(sysInfoFile);
            File.Delete(screenshotFile);
        }

        private async Task<string> SaveScreenshot(string screenshotFile)
        {
            Argument.IsNotNullOrEmpty(() => screenshotFile);

            var mainWindow = Application.Current.MainWindow;
            var image = await _screenCaptureService.CaptureWindowImage(mainWindow);
            image.Save(screenshotFile, ImageFormat.Jpeg);
            return screenshotFile;
        }

        private string SaveSysInfo(string sysInfoFile)
        {
            Argument.IsNotNullOrEmpty(() => sysInfoFile);

            var systemInfo = _systemInfoService.GetSystemInfo().AsParallel();
            var keyValuePairs = systemInfo.ToDictionary(x => x.Key, x => x.Value);
            SaveSystemInfo(sysInfoFile, keyValuePairs);
            return sysInfoFile;
        }

        private static void SaveSystemInfo(string fileName, Dictionary<string, string> sysInfoDictionary)
        {
            Argument.IsNotNullOrEmpty(() => fileName);
            Argument.IsNotNull(() => sysInfoDictionary);

            var serilizer = new XmlSerializer(typeof(Dictionary<string, string>));

            using (var fileStream = new FileStream(fileName, FileMode.Open))
            {
                serilizer.Serialize(fileStream, sysInfoDictionary);
            }
        }
    }
}