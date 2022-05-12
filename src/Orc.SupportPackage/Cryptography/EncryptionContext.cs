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

            PassPhrase = context.PassPhrase;
            SaltValue = context.SaltValue;
            PasswordIterations = context.PasswordIterations;
            InitVector = context.InitVector;
            KeySize = context.KeySize;
        }

        public EncryptionContext(string passPhrase, string saltValue, int? passwordIterations, string initVector, int? keySize)
        {
            PassPhrase = passPhrase;
            SaltValue = saltValue;
            PasswordIterations = passwordIterations;
            InitVector = initVector;
            KeySize = keySize;
        }

        public string PassPhrase { get; set; }

        public string SaltValue { get; set; }

        public int? PasswordIterations { get; set; }

        public string InitVector { get; set; }

        public int? KeySize { get; set; }

        public IProgress<long> Progress { get; set; }
    }
}
