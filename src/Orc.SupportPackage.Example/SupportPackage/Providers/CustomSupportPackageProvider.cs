// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomSupportPackageProvider.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage.Example
{
    using System.Threading.Tasks;
    using Catel;
    using Catel.IoC;
    using Catel.Windows.Interactivity;
    using FileSystem;

    public class CustomSupportPackageProvider : SupportPackageProviderBase
    {
        public override async Task ProvideAsync(ISupportPackageContext supportPackageContext)
        {
            Argument.IsNotNull(() => supportPackageContext);

            var file = supportPackageContext.GetFile("testfile.txt");

            var fileService = this.GetDependencyResolver().Resolve<IFileService>();

            fileService.WriteAllText(file, "custom suppport package contents");

            fileService.WriteAllText(supportPackageContext.GetFile("testfile.exe"), "An exe file as custom package contents");
            fileService.WriteAllText(supportPackageContext.GetFile("testfile.dll"), "An dll file as custom package contents");
            fileService.WriteAllText(supportPackageContext.GetFile("testfile.exe.config"), "An config file as custom package contents");
        }
    }
}