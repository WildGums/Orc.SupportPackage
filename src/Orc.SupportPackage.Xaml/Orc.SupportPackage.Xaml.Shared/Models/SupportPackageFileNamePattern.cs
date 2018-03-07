// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageFileNamePattern.cs" company="WildGums">
//   Copyright (c) 2008 - 2017 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System.Linq;

    using Catel;
    using Catel.Fody;

    public class SupportPackageFileNamePattern : SupportPackageFileSystemArtifact
    {
        #region Constructors
        public SupportPackageFileNamePattern(string title, string[] fileNamePatterns, bool includeInSupportPackage = true)
            : base((title + " " + fileNamePatterns.Aggregate("(", (current, fileExtension) => current + fileExtension + " | ").TrimEnd('|', ' ') + ")").Trim(), includeInSupportPackage)
        {
            Argument.IsNotNullOrEmptyArray(() => fileNamePatterns);

            FileNamePatterns = fileNamePatterns;
        }

        #endregion

        #region Properties
        [NoWeaving]
        public string[] FileNamePatterns { get; }
        #endregion
    }
}