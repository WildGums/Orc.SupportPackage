// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISupportPackageService.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System.Threading.Tasks;

    public interface ISupportPackageService
    {
        Task<bool> CreateSupportPackageAsync(string zipFileName);
    }
}