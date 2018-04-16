// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rect.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int left;

        public int top;

        public int right;

        public int bottom;
    }
}