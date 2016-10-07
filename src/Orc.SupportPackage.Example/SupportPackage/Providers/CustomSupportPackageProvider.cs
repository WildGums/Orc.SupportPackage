﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomSupportPackageProvider.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage.Example
{
    using System.IO;
    using System.Threading.Tasks;
    using Catel;

    public class CustomSupportPackageProvider : SupportPackageProviderBase
    {
        public override async Task ProvideAsync(ISupportPackageContext supportPackageContext)
        {
            Argument.IsNotNull(() => supportPackageContext);

            var file = supportPackageContext.GetFile("testfile.txt");
            
            File.WriteAllText(file, "custom suppport package contents");
        }
    }
}