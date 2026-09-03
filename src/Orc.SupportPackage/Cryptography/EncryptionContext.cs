namespace Orc.SupportPackage
{
    using Catel;

    public class EncryptionContext
    {
        public EncryptionContext()
        {

        }

        public EncryptionContext(EncryptionContext context)
        {
            Argument.IsNotNull(() => context);

            PublicKey = context.PublicKey;
            PrivateKeyPath = context.PrivateKeyPath;
        }

        public EncryptionContext(string publicKey)
        {
            PublicKey = publicKey;
        }

        /// <summary>
        /// Base64-encrypted xml print of generated public key
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// Path to private key on disk
        /// </summary>
        public string PrivateKeyPath { get; set; }
    }
}
