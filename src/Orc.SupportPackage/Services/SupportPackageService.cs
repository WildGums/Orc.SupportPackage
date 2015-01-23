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

    using Ionic.Zip;

    using Orc.SystemInfo.Services;

    internal class SupportPackageService : ISupportPackageService
    {
        private readonly ISystemInfoService _systemInfoService;

        private readonly IScreenCaptureService _screenCaptureService;

        public SupportPackageService(ISystemInfoService systemInfoService, IScreenCaptureService screenCaptureService)
        {
            _systemInfoService = systemInfoService;
            _screenCaptureService = screenCaptureService;
        }

        public async Task CreateSupportPackage(string zipFileName)
        {
            var applicationDataDirectory = Catel.IO.Path.GetApplicationDataDirectory();

            var systemInfo = _systemInfoService.GetSystemInfo().AsParallel();
            var keyValuePairs = systemInfo.ToDictionary(x => x.Key, x => x.Value);            
            var sysInfoFile = Path.Combine(applicationDataDirectory, "sysinfo.xml");
            SaveSystemInfo(sysInfoFile, keyValuePairs);

            var mainWindow = Application.Current.MainWindow;
            var image = await _screenCaptureService.CaptureWindowImage(mainWindow);
            var screenshotFile = Path.Combine(applicationDataDirectory, "screenshot.jpg");
            image.Save(screenshotFile, ImageFormat.Jpeg);

            using (var zipFile = new ZipFile())
            {
                zipFile.AddDirectory(applicationDataDirectory, null);
                zipFile.Save(zipFileName);
            }

            File.Delete(sysInfoFile);
            File.Delete(screenshotFile);
        }

        private static void SaveSystemInfo(string fileName, Dictionary<string, string> sysInfoDictionary)
        {
            var serilizer = new XmlSerializer(typeof(Dictionary<string, string>));

            using (var fileStream = new FileStream(fileName, FileMode.Open))
            {
                serilizer.Serialize(fileStream, sysInfoDictionary);
            }
        }
    }
}