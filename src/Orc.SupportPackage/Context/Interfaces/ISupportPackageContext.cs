namespace Orc.SupportPackage
{
    public interface ISupportPackageContext
    {
        string RootDirectory { get; }

        string GetDirectory(string relativeDirectoryName);
        string GetFile(string relativeFilePath);
    }
}
