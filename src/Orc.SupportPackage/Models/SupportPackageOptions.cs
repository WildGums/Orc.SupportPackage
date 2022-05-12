namespace Orc.SupportPackage
{
    using System.Text;

    /// <summary>
    /// Contains an additional options on extending support package
    /// </summary>
    public class SupportPackageOptions
    {
        /// <summary>
        /// The description metadata builder will be written to SupportPackageOptions.txt
        /// </summary>
        public StringBuilder DescriptionBuilder { get; set; }

        public string[] FileSystemPaths { get; set; }
    }
}
