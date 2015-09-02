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
    using System.Windows.Threading;
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
        public async Task<bool> CreateSupportPackageAsync(string zipFileName)
        {
            Argument.IsNotNullOrEmpty(() => zipFileName);

            var result = true;

            try
            {
                Log.Info("Creating support package");

                using (var supportPackageContext = new SupportPackageContext())
                {
                    // Note: screenshot first, see remarks in screenshot method
                    var screenshotFileName = supportPackageContext.GetFile("screenshot.jpg");
                    await CaptureWindowAndSaveAsync(screenshotFileName);

                    var systemInfoXmlFileName = supportPackageContext.GetFile("systeminfo.xml");
                    var systemInfoTxtFileName = supportPackageContext.GetFile("systeminfo.txt");
                    await GetAndSaveSystemInformationAsync(systemInfoXmlFileName, systemInfoTxtFileName);

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
                            await provider.ProvideAsync(supportPackageContext);
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

        private async Task CaptureWindowAndSaveAsync(string screenshotFile)
        {
            Argument.IsNotNullOrEmpty(() => screenshotFile);

            // Note: we cannot use InvokeAsync here because it might cause deadlocks. Therefore we just dispatcher and 
            // hope it's ready before the package is created. Worst case it doesn't contain the screenshot.
            var application = Application.Current;
            if (application == null)
            {
                Log.Debug("Application.Current is null, cannot create screenshot");
                return;
            }

            var dispatcher = application.Dispatcher;
            
#pragma warning disable CS4014
            dispatcher.BeginInvoke(new Action(() =>
            {
                var mainWindow = application.MainWindow;
                if (mainWindow == null)
                {
                    Log.Debug("Application.Current.MainWindow is null, cannot create screenshot");
                    return;
                }

                Log.Debug("Creating screenshot for support package");

                var image = _screenCaptureService.CaptureWindowImage(mainWindow);
                image.Save(screenshotFile, ImageFormat.Jpeg);
            }));
#pragma warning restore CS4014
        }

        private Task GetAndSaveSystemInformationAsync(string xmlFileName, string textFileName)
        {
            Argument.IsNotNullOrEmpty(() => xmlFileName);
            Argument.IsNotNullOrEmpty(() => textFileName);

            return TaskHelper.Run(() =>
            {
                Log.Debug("Gathering system info for support package");

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
            }, true);
        }
    }
}