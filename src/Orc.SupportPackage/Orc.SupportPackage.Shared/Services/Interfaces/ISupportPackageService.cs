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
        #region Methods
        [ObsoleteEx(RemoveInVersion = "2.0", TreatAsErrorFromVersion = "1.5", ReplacementTypeOrMember = "CreateSupportPackageAsync(zipFileName, directories, excludeFileNamePatterns)")]
        Task<bool> CreateSupportPackageAsync(string zipFileName);

        [ObsoleteEx(RemoveInVersion = "2.0", TreatAsErrorFromVersion = "1.5", ReplacementTypeOrMember = "CreateSupportPackageAsync(zipFileName, directories, excludeFileNamePatterns)")]
        Task<bool> CreateSupportPackageAsync(string zipFileName, string[] excludeFileNamePatterns);

        Task<bool> CreateSupportPackageAsync(string zipFileName, string[] directories, string[] excludeFileNamePatterns);
        #endregion
    }
}