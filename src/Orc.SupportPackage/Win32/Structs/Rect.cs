﻿namespace Orc.SupportPackage.Win32;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct RECT
{
    public int left;

    public int top;

    public int right;

    public int bottom;
}
