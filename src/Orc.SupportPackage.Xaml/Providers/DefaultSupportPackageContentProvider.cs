namespace Orc.SupportPackage;

using System;
using System.Collections.Generic;
using Catel;
using Catel.Services;

public sealed class DefaultSupportPackageContentProvider : ISupportPackageContentProvider
{
    private readonly ILanguageService _languageService;

    public DefaultSupportPackageContentProvider(ILanguageService languageService)
    {
        ArgumentNullException.ThrowIfNull(languageService);

        _languageService = languageService;
    }

    public IReadOnlyList<SupportPackageFileSystemArtifact> GetSupportPackageFileSystemArtifacts()
    {
        return new[]
        {
            new SupportPackageFileNamePattern(_languageService.GetRequiredString("SupportPackage_SupportPackageFileType_SystemInformation_Title"), new[] { "systeminfo.json", "systeminfo.txt" }),
            new SupportPackageFileNamePattern(_languageService.GetRequiredString("SupportPackage_SupportPackageFileType_ExecutableFiles_Title"), new[] { "*.exe", "*.dll" }, false),
            new SupportPackageFileNamePattern(_languageService.GetRequiredString("SupportPackage_SupportPackageFileType_ConfigurationFiles_Title"), new[] { "*.config" }),
            new SupportPackageFileNamePattern(_languageService.GetRequiredString("SupportPackage_SupportPackageFileType_LogFiles_Title"), new[] { "*.log" }),
            new SupportPackageFileNamePattern(_languageService.GetRequiredString("SupportPackage_SupportPackageFileType_TextFiles_Title"), new[] { "*.txt" }),
            new SupportPackageFileNamePattern(_languageService.GetRequiredString("SupportPackage_SupportPackageFileType_ImageFiles_Title"), new[] { "*.jpg", "*.bmp" })
        };
    }
}
