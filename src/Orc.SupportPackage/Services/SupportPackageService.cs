// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageService.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Xml.Serialization;
    using SystemInfo;
    using Catel;
    using Catel.Collections;
    using Catel.Logging;
    using Ionic.Zip;
    using Ionic.Zlib;

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

                using (var temporaryFilesContext = new TemporaryFilesContext())
                {
                    var systemInfoXmlFileName = temporaryFilesContext.GetFile("systeminfo.xml");
                    var systemInfoTxtFileName = temporaryFilesContext.GetFile("systeminfo.txt");
                    await GetAndSaveSystemInformation(systemInfoXmlFileName, systemInfoTxtFileName);

                    var screenshotFileName = temporaryFilesContext.GetFile("screenshot.jpg");
                    await CaptureWindowAndSave(screenshotFileName);

                    var applicationDataDirectory = Catel.IO.Path.GetApplicationDataDirectory();

                    using (var zipFile = new ZipFile())
                    {
                        zipFile.CompressionLevel = CompressionLevel.BestCompression;

                        zipFile.AddDirectory(applicationDataDirectory, "AppData");
                        zipFile.AddFile(systemInfoXmlFileName, string.Empty);
                        zipFile.AddFile(systemInfoTxtFileName, string.Empty);
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

        private async Task GetAndSaveSystemInformation(string xmlFileName, string textFileName)
        {
            Argument.IsNotNullOrEmpty(() => xmlFileName);
            Argument.IsNotNullOrEmpty(() => textFileName);

            var systemInfo = await _systemInfoService.GetSystemInfo();

            // Xml
            var serializer = new XmlSerializer(systemInfo.GetType());
            using (var fileStream = new FileStream(xmlFileName, FileMode.OpenOrCreate))
            {
                serializer.Serialize(fileStream, systemInfo);
            }

            // Plain
            var stringBuilder = new StringBuilder();
            systemInfo.ForEach(x => stringBuilder.AppendLine(x.ToString()));
            File.WriteAllText(textFileName, stringBuilder.ToString());
        }
    }
}