namespace Orc.SupportPackage
{
    using System.Collections.Generic;

    public class SupportPackageBuilderContext
    {
        /// <summary>
        /// Name of support package file
        /// </summary>
        public string FileName { get; set; }

        public List<SupportPackageFileSystemArtifact> Artifacts { get; set; }

        public bool IsEncrypted { get; set; }

        public EncryptionContext EncryptionContext { get; set; }
    }
}
