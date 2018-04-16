// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISupportPackageBuilderService.cs" company="WildGums">
//   Copyright (c) 2008 - 2018 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.SupportPackage
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISupportPackageBuilderService
    {
        #region Methods
        Task<bool> CreateSupportPackageAsync(string fileName, List<SupportPackageFileSystemArtifact> artifacts);
        #endregion
    }
}