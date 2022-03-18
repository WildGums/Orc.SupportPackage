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
        public IEnumerable<SupportPackageFileSystemArtifact> GetSupportPackageFileSystemArtifacts()
        {
            var demoProjectPath = Path.Combine(GetType().GetAssemblyEx().GetDirectory(), "Orc.SupportPackage.Example");
            Directory.CreateDirectory(demoProjectPath);


            using (var file = File.Create(Path.Combine(demoProjectPath, "Orc.SupportPackage.Example.demoproject")))
            {
                file.Flush();
            }

            using (var file = File.Create(Path.Combine(demoProjectPath, "Orc.SupportPackage.Example.demoproject.data")))
            {
                file.Flush();
            }

            yield return new SupportPackageDirectory("Orc.SupportPackage.Example Demo Directory", demoProjectPath);
        }
    }
}
