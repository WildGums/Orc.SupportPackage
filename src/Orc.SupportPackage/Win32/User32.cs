namespace Orc.SupportPackage.Win32;

using System;
using System.Runtime.InteropServices;

internal static class User32
{
    [DllImport("user32.dll")]
    internal static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    internal static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("user32.dll")]
    internal static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
}
