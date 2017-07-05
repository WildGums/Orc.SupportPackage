// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageFileSystemArtifact.cs" company="WildGums">
//   Copyright (c) 2008 - 2017 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using Catel;
    using Catel.Data;

    public class SupportPackageFileSystemArtifact : ModelBase
    {
        #region Constructors
        protected SupportPackageFileSystemArtifact(string title, bool includeInSupportPackage)
        {
            Argument.IsNotNullOrWhitespace(() => title);

            Title = title;
            IncludeInSupportPackage = includeInSupportPackage;
        }

        #endregion

        #region Properties
        public string Title { get; set; }

        public bool IncludeInSupportPackage { get; set; }
        #endregion
    }
}