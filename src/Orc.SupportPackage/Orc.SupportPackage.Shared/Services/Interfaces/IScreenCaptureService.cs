// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IScreenCaptureService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
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