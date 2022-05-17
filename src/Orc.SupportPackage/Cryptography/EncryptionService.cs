namespace Orc.SupportPackage
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Logging;
    using Orc.FileSystem;

    public class EncryptionService : IEncryptionService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private const int KeySize = 2048;

        private readonly IFileService _fileService;

        private const string PemLeadingPrivate = "-----BEGIN RSA PRIVATE KEY-----";
        private const string PemTrailingPrivate = "-----END RSA PRIVATE KEY-----";
        private const string PemLeadingPublic = "-----BEGIN RSA PUBLIC KEY-----";
        private const string PemTrailingPublic = "-----END RSA PUBLIC KEY-----";

        public EncryptionService(IFileService fileService)
        {
            Argument.IsNotNull(() => fileService);

            _fileService = fileService;
        }

        public async Task EncryptAsync(
            Stream sourceStream,
            Stream targetStream,
            EncryptionContext content)
        {
            try
            {
                Argument.IsNotNullOrEmpty(() => content.PublicKey);

                using (var rsa = CreateAlghorithm())
                {
                    var publicRaw = Convert.FromBase64String(content.PublicKey);

                    rsa.ImportRSAPublicKey(publicRaw, out _);

                    using (var memoryStream = new MemoryStream())
                    {
                        sourceStream.CopyTo(memoryStream);

                        var buffer = memoryStream.ToArray();
                        var encryptedByres = rsa.Encrypt(buffer, RSAEncryptionPadding.OaepSHA256);

                        targetStream.Write(encryptedByres, 0, encryptedByres.Length);
                        await targetStream.FlushAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to encrypt");

                throw;
            }
        }

        public async Task DecryptAsync(
            Stream sourceStream,
            Stream targetStream,
            EncryptionContext context)
        {
            try
            {
                Argument.IsNotNullOrEmpty(() => context.PrivateKeyPath);

                using (var rsa = CreateAlghorithm())
                {
                    // Read secret keeping only the payload of the key 
                    var pemText = await _fileService.ReadAllTextAsync(context.PrivateKeyPath);
                    pemText = pemText.Replace(PemTrailingPrivate, "");
                    pemText = pemText.Replace(PemLeadingPublic, "");

                    var secretRaw = Convert.FromBase64String(pemText);

                    rsa.ImportRSAPrivateKey(secretRaw, out _);

                    using (var memoryStream = new MemoryStream())
                    {
                        sourceStream.CopyTo(memoryStream);

                        var buffer = memoryStream.ToArray();
                        var decryptedBytes = rsa.Decrypt(buffer, RSAEncryptionPadding.OaepSHA256);

                        targetStream.Write(decryptedBytes, 0, decryptedBytes.Length);
                        await targetStream.FlushAsync();
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to decrypt");
                throw;
            }
        }

        protected virtual RSA CreateAlghorithm()
        {
            var rsa = new RSACryptoServiceProvider(KeySize);
            return rsa;
        }

        public void Generate(string secretPath, string publicKeyPath)
        {
            using (var rsa = CreateAlghorithm())
            {
                var privateKey = rsa.ExportRSAPrivateKey();
                var publicKey = rsa.ExportRSAPublicKey();

                var privateKeyPem = Convert.ToBase64String(privateKey);
                var publicKeyPem = Convert.ToBase64String(publicKey);

                var stringBuilder = new StringBuilder();

                stringBuilder.AppendLine(PemLeadingPrivate);
                stringBuilder.AppendLine(privateKeyPem);
                stringBuilder.AppendLine(PemTrailingPrivate);

                var privateTextPem = stringBuilder.ToString();

                stringBuilder.Clear();

                stringBuilder.AppendLine(PemLeadingPublic);
                stringBuilder.AppendLine(publicKeyPem);
                stringBuilder.AppendLine(PemTrailingPublic);

                var publicTextPem = stringBuilder.ToString();

                _fileService.WriteAllText(secretPath, privateTextPem);
                _fileService.WriteAllText(publicKeyPath, publicTextPem);
            }
        }
    }
}
