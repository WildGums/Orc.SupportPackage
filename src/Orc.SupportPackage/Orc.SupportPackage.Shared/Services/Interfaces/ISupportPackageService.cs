// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISupportPackageService.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    public interface ISupportPackageService
    {
        bool CreateSupportPackage(string zipFileName);
    }
}