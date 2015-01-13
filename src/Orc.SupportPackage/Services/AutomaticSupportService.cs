// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutomaticSupportService.cs" company="Orchestra development team">
//   Copyright (c) 2008 - 2014 Orchestra development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.AutomaticSupport
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Logging;
    using Catel.Services;

    public class AutomaticSupportService : IAutomaticSupportService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IDispatcherService _dispatcherService;
        private readonly IProcessService _processService;
        private readonly DateTime _startedTime;

        public AutomaticSupportService(IProcessService processService, IDispatcherService dispatcherService)
        {
            Argument.IsNotNull(() => processService);
            Argument.IsNotNull(() => dispatcherService);

            _processService = processService;
            _dispatcherService = dispatcherService;

            _startedTime = DateTime.Now;
        }

        public string SupportUrl { get; set; }

        public event EventHandler<ProgressChangedEventArgs> DownloadProgressChanged;
        public event EventHandler<EventArgs> DownloadCompleted;
        public event EventHandler<EventArgs> SupportAppClosed;

        public async Task DownloadAndRun()
        {
            if (string.IsNullOrWhiteSpace(SupportUrl))
            {
                Log.ErrorAndThrowException<InvalidOperationException>("Please initialize the service by setting the SupportUrl property");
            }

            Log.Info("Downloading support app from '{0}'", SupportUrl);

            var webClient = new WebClient();

            webClient.DownloadProgressChanged += OnWebClientOnDownloadProgressChanged;
            webClient.DownloadDataCompleted += OnWebClientOnDownloadDataCompleted;

            var data = await webClient.DownloadDataTaskAsync(SupportUrl);

            Log.Info("Support app is downloaded, storing file in temporary folder");

            var tempDirectory = Path.Combine(Path.GetTempPath(), "Orc_AutomaticSupport", DateTime.Now.ToString("yyyyMMddHHmmss"));
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }

            var tempFile = Path.Combine(tempDirectory, "SupportApp.exe");

            File.WriteAllBytes(tempFile, data);

            Log.Info("Running support app");

            _processService.StartProcess(tempFile, processCompletedCallback: exitCode =>
            {
                _dispatcherService.BeginInvoke(() => SupportAppClosed.SafeInvoke(this));
            });
        }

        private void OnWebClientOnDownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            var webClient = (WebClient) sender;

            webClient.DownloadProgressChanged -= OnWebClientOnDownloadProgressChanged;
            webClient.DownloadDataCompleted -= OnWebClientOnDownloadDataCompleted;

            DownloadCompleted.SafeInvoke(this);
        }

        private void OnWebClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var remainingTime = CalculateEta(_startedTime, Convert.ToInt32(e.TotalBytesToReceive), Convert.ToInt32(e.BytesReceived));
            DownloadProgressChanged.SafeInvoke(this, new ProgressChangedEventArgs(e.ProgressPercentage, remainingTime));
        }

        private TimeSpan CalculateEta(DateTime startedTime, int totalBytesToReceive, int bytesReceived)
        {
            var duration = (int) (DateTime.Now - startedTime).TotalSeconds;
            if (duration == 0)
            {
                return new TimeSpan(0, 0, 0);
            }

            var bytesPerSecond = bytesReceived/duration;
            if (bytesPerSecond == 0)
            {
                return new TimeSpan(0, 0, 0);
            }

            var secondsRemaining = (totalBytesToReceive - bytesReceived)/bytesPerSecond;

            return new TimeSpan(0, 0, secondsRemaining);
        }
    }
}