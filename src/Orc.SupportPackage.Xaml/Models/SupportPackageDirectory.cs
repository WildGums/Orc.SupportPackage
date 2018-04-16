// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageDirectory.cs" company="WildGums">
//   Copyright (c) 2008 - 2017 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using Catel;
    using Catel.Fody;

    public class SupportPackageDirectory : SupportPackageFileSystemArtifact
    {
        #region Constructors
        public SupportPackageDirectory(string title, string directoryName, bool includeInSupportPackage = true)
            : base(title, includeInSupportPackage)
        {
            Argument.IsNotNullOrWhitespace(() => directoryName);

            DirectoryName = directoryName;
        }

        #endregion

        #region Properties
        [NoWeaving]
        public string DirectoryName { get; }
        #endregion
    }
}