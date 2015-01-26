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

    public class SupportPackageService : ISupportPackageService
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

            var result = true;

            try
            {
                Log.Info("Creating support package");
              
                using (var tmpHelper = new TemporaryFilesHelper())
                {
                    var sysinfoFileName = tmpHelper.RegisterFileName("sysinfo.xml");
                    GetAndSaveSystemInformation(sysinfoFileName);

                    var screenshotFileName = tmpHelper.RegisterFileName("screenshot.jpg");
                    await CaptureWindowAndSave(screenshotFileName);

                    using (var zipFile = new ZipFile())
                    {
                        var applicationDataDirectory = Catel.IO.Path.GetApplicationDataDirectory();
                        zipFile.AddDirectory(applicationDataDirectory, "AppData");
                        zipFile.AddFile(sysinfoFileName, string.Empty);
                        zipFile.AddFile(screenshotFileName, string.Empty);
                        zipFile.Save(zipFileName);
                    }
                }                                              

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

            var directoryName = Catel.IO.Path.GetDirectoryName(sysInfoFile);

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            using (var fileStream = new FileStream(sysInfoFile, FileMode.OpenOrCreate))
            {
                serilizer.Serialize(fileStream, systemInfo);
            }
        }
    }
}