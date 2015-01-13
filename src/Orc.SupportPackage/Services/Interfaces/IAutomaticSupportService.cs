// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAutomaticSupportService.cs" company="Orchestra development team">
//   Copyright (c) 2008 - 2014 Orchestra development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.AutomaticSupport
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// The automatic support service.
    /// </summary>
    public interface IAutomaticSupportService
    {
        /// <summary>
        /// Gets or sets the support URL. This url should point to a file that can be downloaded and executed.
        /// </summary>
        /// <value>The support URL.</value>
        string SupportUrl { get; set; }

        /// <summary>
        /// Occurs when the download progress has changed.
        /// </summary>
        event EventHandler<ProgressChangedEventArgs> DownloadProgressChanged;

        /// <summary>
        /// Occurs when the download is completed.
        /// </summary>
        event EventHandler<EventArgs> DownloadCompleted;

        /// <summary>
        /// Occurs when when the support app is closed.
        /// </summary>
        event EventHandler<EventArgs> SupportAppClosed;

        /// <summary>
        /// Downloads the support url file and runs the application.
        /// </summary>
        /// <returns>Task.</returns>
        Task DownloadAndRun();
    }
}