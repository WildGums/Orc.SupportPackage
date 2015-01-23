// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IScreenCaptureService.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage.Services
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Threading.Tasks;

    using Window = System.Windows.Window;

    public interface IScreenCaptureService
    {
        Task<Image> CaptureWindowImage(Window window);//, string fileName, ImageFormat imageFormat);
    }
}