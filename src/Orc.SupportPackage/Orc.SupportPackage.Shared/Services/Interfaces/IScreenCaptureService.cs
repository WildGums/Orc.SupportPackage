// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IScreenCaptureService.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System.Drawing;
    using System.Windows;

    public interface IScreenCaptureService
    {
        Image CaptureWindowImage(Window window);
    }
}