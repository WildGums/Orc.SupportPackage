// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageService.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System;
    using System.Collections.Generic;
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
    using Catel.IoC;
    using Catel.Logging;
    using Catel.Reflection;
    using Catel.Threading;
    using Ionic.Zip;
    using Ionic.Zlib;
    using MethodTimer;

    public class SupportPackageService : ISupportPackageService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IScreenCaptureService _screenCaptureService;
        private readonly ITypeFactory _typeFactory;
        private readonly ISystemInfoService _systemInfoService;

        public SupportPackageService(ISystemInfoService systemInfoService, IScreenCaptureService screenCaptureService,
            ITypeFactory typeFactory)
        {
            Argument.IsNotNull(() => systemInfoService);
            Argument.IsNotNull(() => screenCaptureService);
            Argument.IsNotNull(() => typeFactory);

            _systemInfoService = systemInfoService;
            _screenCaptureService = screenCaptureService;
            _typeFactory = typeFactory;
        }

        [Time]
        public bool CreateSupportPackage(string zipFileName)
        {
            Argument.IsNotNullOrEmpty(() => zipFileName);

            var result = true;

            try
            {
                Log.Info("Creating support package");

                using (var supportPackageContext = new SupportPackageContext())
                {
                    var systemInfoXmlFileName = supportPackageContext.GetFile("systeminfo.xml");
                    var systemInfoTxtFileName = supportPackageContext.GetFile("systeminfo.txt");
                    GetAndSaveSystemInformation(systemInfoXmlFileName, systemInfoTxtFileName);

                    var screenshotFileName = supportPackageContext.GetFile("screenshot.jpg");
                    CaptureWindowAndSave(screenshotFileName);

                    var supportPackageProviderTypes = (from type in TypeCache.GetTypes()
                                                       where !type.IsAbstractEx() && type.IsClassEx() &&
                                                             type.ImplementsInterfaceEx<ISupportPackageProvider>()
                                                       select type).ToList();

                    foreach (var supportPackageProviderType in supportPackageProviderTypes)
                    {
                        try
                        {
                            Log.Debug("Gathering support package info from '{0}'", supportPackageProviderType.FullName);

                            var provider = (ISupportPackageProvider)_typeFactory.CreateInstance(supportPackageProviderType);
                            provider.Provide(supportPackageContext);
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, "Failed to gather support package info from '{0}'. Info will be excluded from the package", supportPackageProviderType.FullName);
                        }
                    }

                    var applicationDataDirectory = Catel.IO.Path.GetApplicationDataDirectory();

                    using (var zipFile = new ZipFile())
                    {
                        zipFile.CompressionLevel = CompressionLevel.BestCompression;

                        zipFile.AddDirectory(applicationDataDirectory, "AppData");
                        zipFile.AddDirectory(supportPackageContext.RootDirectory, string.Empty);

                        zipFile.Save(zipFileName);
                    }
                }

                Log.Info("Support package created");
            }
            catch (Exception ex)
            {
                result = false;
                Log.Error(ex, "Error while creating support package");
            }

            return result;
        }

        private void CaptureWindowAndSave(string screenshotFile)
        {
            Argument.IsNotNullOrEmpty(() => screenshotFile);

            var mainWindow = Application.Current.MainWindow;
            var image = _screenCaptureService.CaptureWindowImage(mainWindow);
            image.Save(screenshotFile, ImageFormat.Jpeg);
        }

        private void GetAndSaveSystemInformation(string xmlFileName, string textFileName)
        {
            Argument.IsNotNullOrEmpty(() => xmlFileName);
            Argument.IsNotNullOrEmpty(() => textFileName);

            var systemInfo = _systemInfoService.GetSystemInfo();

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