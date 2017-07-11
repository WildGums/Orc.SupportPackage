// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomSupportPackageContentProvider.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage.Example.SupportPackage.Providers
{
    using System.Collections.Generic;
    using System.IO;

    using Catel.Reflection;

    public sealed class CustomSupportPackageContentProvider : ISupportPackageContentProvider
    {
        #region Methods
        public IEnumerable<SupportPackageFileSystemArtifact> GetSupportPackageFileSystemArtifacts()
        {
            var demoProjectPath = Path.Combine(this.GetType().GetAssemblyEx().GetDirectory(), "Orc.SupportPackage.Example");
            Directory.CreateDirectory(demoProjectPath);
            File.Create(Path.Combine(demoProjectPath, "Orc.SupportPackage.Example.demoproject")).Flush();
            File.Create(Path.Combine(demoProjectPath, "Orc.SupportPackage.Example.demoproject.data")).Flush();
            yield return new SupportPackageDirectory("Orc.SupportPackage.Example Demo Directory", demoProjectPath);
        }

        #endregion
    }
}