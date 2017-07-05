// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISupportPackageContentProvider.cs" company="WildGums">
//   Copyright (c) 2008 - 2017 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System.Collections.Generic;

    public interface ISupportPackageContentProvider
    {
        IEnumerable<SupportPackageFileSystemArtifact> GetSupportPackageFileSystemArtifacts();
    }
}