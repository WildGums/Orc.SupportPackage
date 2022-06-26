// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISupportPackageService.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System.Threading.Tasks;
    using MethodTimer;

    public interface ISupportPackageService
    {
        #region Methods
        Task<bool> CreateSupportPackageAsync(string zipFileName, string[] directories, string[] excludeFileNamePatterns);

        Task<bool> CreateSupportPackageAsync(SupportPackageContext supportPackageContext);
        #endregion
    }
}
