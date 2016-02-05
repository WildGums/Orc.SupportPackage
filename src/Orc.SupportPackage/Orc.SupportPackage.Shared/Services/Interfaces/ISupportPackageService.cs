// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISupportPackageService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
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