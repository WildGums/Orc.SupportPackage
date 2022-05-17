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

                    sourceStream.Seek(0, SeekOrigin.Begin);

                    // To support rsa + sha each read block should be KeySize - SHA overhead size (66 bytes)
                    var buffer = new byte[rsa.KeySize / 8 - 66];
                    int bytesRead = 0;

                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        var encryptedBytes = rsa.Encrypt(buffer[0..(bytesRead - 1)], RSAEncryptionPadding.OaepSHA256);
                        targetStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                    }

                    await targetStream.FlushAsync();

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
                    var pemText = await ReadPrivateKeyFromPemFileAsync(context.PrivateKeyPath);

                    var secretRaw = Convert.FromBase64String(pemText);

                    rsa.ImportRSAPrivateKey(secretRaw, out _);

                    // To support rsa + sha each read block should be KeySize - SHA overhead size (66 bytes)
                    var buffer = new byte[rsa.KeySize / 8 - 66];
                    int bytesRead = 0;

                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        var decryptedBytes = rsa.Decrypt(new byte[1], RSAEncryptionPadding.OaepSHA256);
                        targetStream.Write(decryptedBytes, 0, decryptedBytes.Length);
                    }

                    await targetStream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to decrypt");
                throw;
            }
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

        public async Task<string> ReadPublicKeyFromPemFileAsync(string keyPath)
        {
            var pemText = await _fileService.ReadAllTextAsync(keyPath);
            pemText = pemText.Replace(PemTrailingPublic, "");
            pemText = pemText.Replace(PemLeadingPublic, "");

            pemText = pemText.Trim('\r', '\n');

            return pemText;
        }

        public async Task<string> ReadPrivateKeyFromPemFileAsync(string keyPath)
        {
            var pemText = await _fileService.ReadAllTextAsync(keyPath);
            pemText = pemText.Replace(PemTrailingPrivate, "");
            pemText = pemText.Replace(PemLeadingPrivate, "");

            pemText = pemText.Trim('\r', '\n');

            return pemText;
        }

        protected virtual RSA CreateAlghorithm()
        {
            var rsa = new RSACng(KeySize);
            return rsa;
        }
    }
}
