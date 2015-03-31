// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISupportPackageContext.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
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