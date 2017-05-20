// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageFileType.cs" company="WildGums">
//   Copyright (c) 2008 - 2017 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Orc.SupportPackage
{
    using System.Linq;
    using Catel;
    using Catel.Data;
    using Catel.Fody;

    public class SupportPackageFileType : ModelBase
    {
        public SupportPackageFileType(string title, string[] fileNamePatterns, bool includeInSupportPackage = true)
        {
            Argument.IsNotNullOrWhitespace(() => title);
            Argument.IsNotNullOrEmptyArray(() => fileNamePatterns);

            string patterns = fileNamePatterns.Aggregate("(", (current, fileExtension) => current + fileExtension + " | ").TrimEnd('|', ' ') + ")";

            Title = title + " " + patterns;
            FileNamePatterns = fileNamePatterns;
            IncludeInSupportPackage = includeInSupportPackage;
        }

        public string Title { get; }

        [NoWeaving]
        public string[] FileNamePatterns { get; }

        public bool IncludeInSupportPackage { get; set; } 
    }
}