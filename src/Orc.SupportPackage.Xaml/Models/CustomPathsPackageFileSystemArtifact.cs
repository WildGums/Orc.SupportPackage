// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomPathsPackageFileSystemArtifact.cs" company="WildGums">
//   Copyright (c) 2008 - 2017 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using Catel;

    /// <summary>
    /// The custom paths package file system artifact.
    /// </summary>
    public class CustomPathsPackageFileSystemArtifact : SupportPackageFileSystemArtifact
    {
        #region Constructors
        public CustomPathsPackageFileSystemArtifact(string title, List<string> paths, bool includeInSupportPackage)
            : base(title, includeInSupportPackage)
        {
            Argument.IsNotNullOrWhitespace(() => title);

            Title = title;
            Paths = paths.AsReadOnly();
            IncludeInSupportPackage = includeInSupportPackage;
        }

        #endregion

        #region Properties
        public ReadOnlyCollection<string> Paths { get; }
        #endregion
    }
}