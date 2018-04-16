// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultSupportPackageContentProvider.cs" company="WildGums">
//   Copyright (c) 2008 - 2017 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.SupportPackage
{
    using System.Collections.Generic;
    using Catel;
    using Catel.Services;

    public sealed class DefaultSupportPackageContentProvider : ISupportPackageContentProvider
    {
        #region Fields
        private readonly ILanguageService _languageService;
        #endregion

        #region Constructors
        public DefaultSupportPackageContentProvider(ILanguageService languageService)
        {
            Argument.IsNotNull(() => languageService);
            _languageService = languageService;
        }

        #endregion

        #region Methods
        public IEnumerable<SupportPackageFileSystemArtifact> GetSupportPackageFileSystemArtifacts()
        {
            yield return new SupportPackageFileNamePattern(_languageService.GetString("SupportPackage_SupportPackageFileType_SystemInformation_Title"), new[] {"systeminfo.xml", "systeminfo.txt"});
            yield return new SupportPackageFileNamePattern(_languageService.GetString("SupportPackage_SupportPackageFileType_ExecutableFiles_Title"), new[] {"*.exe", "*.dll"}, false);
            yield return new SupportPackageFileNamePattern(_languageService.GetString("SupportPackage_SupportPackageFileType_ConfigurationFiles_Title"), new[] {"*.config"});
            yield return new SupportPackageFileNamePattern(_languageService.GetString("SupportPackage_SupportPackageFileType_LogFiles_Title"), new[] {"*.log"});
            yield return new SupportPackageFileNamePattern(_languageService.GetString("SupportPackage_SupportPackageFileType_TextFiles_Title"), new[] {"*.txt"});
            yield return new SupportPackageFileNamePattern(_languageService.GetString("SupportPackage_SupportPackageFileType_ImageFiles_Title"), new[] {"*.jpg", "*.bmp"});
        }
        #endregion
    }
}