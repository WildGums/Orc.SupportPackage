// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISupportPackageContext.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    public interface ISupportPackageContext
    {
        string RootDirectory { get; }

        string GetDirectory(string relativeDirectoryName);
        string GetFile(string relativeFilePath);
    }
}