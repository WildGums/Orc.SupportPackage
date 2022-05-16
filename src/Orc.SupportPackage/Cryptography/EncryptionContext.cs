namespace Orc.SupportPackage
{
    using System;
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
            SaltValue = context.SaltValue;
            PasswordIterations = context.PasswordIterations;
            InitVector = context.InitVector;
            KeySize = context.KeySize;
        }

        public EncryptionContext(string publicKey, string saltValue, int? passwordIterations, string initVector, int? keySize)
        {
            PublicKey = publicKey;
            SaltValue = saltValue;
            PasswordIterations = passwordIterations;
            InitVector = initVector;
            KeySize = keySize;
        }

        /// <summary>
        /// Base64-encrypted xml print of generated public key
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// Path to private key on disk
        /// </summary>
        public string PrivateKeyPath { get; set; }

        public string SaltValue { get; set; }

        public int? PasswordIterations { get; set; }

        public string InitVector { get; set; }

        public int? KeySize { get; set; }

        public IProgress<long> Progress { get; set; }
    }
}
