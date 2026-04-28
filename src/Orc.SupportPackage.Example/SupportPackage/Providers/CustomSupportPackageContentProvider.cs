namespace Orc.SupportPackage.Example;

using System.Collections.Generic;
using System.IO;

using Catel.Reflection;

public sealed class CustomSupportPackageContentProvider : ISupportPackageContentProvider
{
    public IReadOnlyList<SupportPackageFileSystemArtifact> GetSupportPackageFileSystemArtifacts()
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

        return new[]
        {
            new SupportPackageDirectory("Orc.SupportPackage.Example Demo Directory", demoProjectPath)
        };
    }
}
