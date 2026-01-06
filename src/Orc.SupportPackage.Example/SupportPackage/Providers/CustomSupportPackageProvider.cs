namespace Orc.SupportPackage.Example;

using System;
using System.Threading.Tasks;
using FileSystem;

public class CustomSupportPackageProvider : SupportPackageProviderBase
{
    private readonly IFileService _fileService;

    public CustomSupportPackageProvider(IFileService fileService)
    {
        _fileService = fileService;
    }

    public override async Task ProvideAsync(ISupportPackageContext supportPackageContext)
    {
        ArgumentNullException.ThrowIfNull(supportPackageContext);

        var file = supportPackageContext.GetFile("testfile.txt");

        _fileService.WriteAllText(file, "custom suppport package contents");

        _fileService.WriteAllText(supportPackageContext.GetFile("testfile.exe"), "An exe file as custom package contents");
        _fileService.WriteAllText(supportPackageContext.GetFile("testfile.dll"), "An dll file as custom package contents");
        _fileService.WriteAllText(supportPackageContext.GetFile("testfile.exe.config"), "An config file as custom package contents");
    }
}
