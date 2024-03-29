﻿namespace Orc.SupportPackage;

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using Win32;

public class ScreenCaptureService : IScreenCaptureService
{
    public Image CaptureWindowImage(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        var windowHandle = new WindowInteropHelper(window).Handle;

        return CaptureWindowImageByHandle(windowHandle);
    }

    private Image CaptureWindowImageByHandle(IntPtr handle)
    {
        var windowRect = GetWindowRect(handle);
        var width = windowRect.right - windowRect.left;
        var height = windowRect.bottom - windowRect.top;

        var img = GetImage(handle, width, height);

        return img;
    }

    private static Image GetImage(IntPtr handle, int width, int height)
    {
        var srcDC = User32.GetWindowDC(handle);
        var destDC = Gdi32.CreateCompatibleDC(srcDC);

        // create a bitmap we can copy it to,
        // using GetDeviceCaps to get the width/height
        var hBbitmap = Gdi32.CreateCompatibleBitmap(srcDC, width, height);

        // select the bitmap object
        var oldHandle = Gdi32.SelectObject(destDC, hBbitmap);

        // bitblt over
        Gdi32.BitBlt(destDC, 0, 0, width, height, srcDC, 0, 0, Gdi32.SRCCOPY);

        // restore selection
        Gdi32.SelectObject(destDC, oldHandle);

        // clean up 
        Gdi32.DeleteDC(destDC);
        User32.ReleaseDC(handle, srcDC);

        var img = Image.FromHbitmap(hBbitmap);

        Gdi32.DeleteObject(hBbitmap);

        return img;
    }

    private static RECT GetWindowRect(IntPtr handle)
    {
        var windowRect = new RECT();
        User32.GetWindowRect(handle, ref windowRect);
        return windowRect;
    }
}
