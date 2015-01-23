// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Rect.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Orc.SupportPackage.Models
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