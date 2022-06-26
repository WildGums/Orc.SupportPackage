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
        private const int RandomizedKeySize = 256; // For data encryption
        private const int IVSize = 128;

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

                using (var rsa = new RSACng(KeySize))
                {
                    var publicRaw = Convert.FromBase64String(content.PublicKey);

                    rsa.ImportRSAPublicKey(publicRaw, out _);

                    sourceStream.Seek(0, SeekOrigin.Begin);

                    // To successfuly encrypt with rsa + sha the message (randomized key) should be less than KeySize - SHA overhead size (66 bytes)
                    // var maxRandomizedKeyLength = rsa.KeySize / 8 - 66

                    using (var symmetric = CreateAlghorithm())
                    {
                        // These parameters should be passed as cipher text (encrypted with rsa)
                        var dataKey = GenerateSymmetricKey(RandomizedKeySize);
                        var iv = GenerateSymmetricKey(IVSize);

                        symmetric.Key = dataKey;
                        symmetric.IV = iv;

                        // Create an encryptor to perform the stream transform.
                        using (var encryptor = symmetric.CreateEncryptor(symmetric.Key, symmetric.IV))
                        {
                            var secretBytes = new byte[symmetric.Key.Length + symmetric.IV.Length];
                            Buffer.BlockCopy(symmetric.IV, 0, secretBytes, 0, symmetric.IV.Length);
                            Buffer.BlockCopy(symmetric.Key, 0, secretBytes, symmetric.IV.Length, symmetric.Key.Length);

                            var cipherSecret = rsa.Encrypt(secretBytes, RSAEncryptionPadding.OaepSHA256);

                            // Write this block in the beggining of cipher text
                            await targetStream.WriteAsync(cipherSecret);

                            using (var cryptoStream = new CryptoStream(targetStream, encryptor, CryptoStreamMode.Write, true))
                            {
                                await sourceStream.CopyToAsync(cryptoStream);
                                await cryptoStream.FlushAsync();
                            }
                        }
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

                using (var rsa = new RSACng(KeySize))
                {
                    // Read secret keeping only the payload of the key 
                    var pemText = await ReadPrivateKeyFromPemFileAsync(context.PrivateKeyPath);

                    var secretRaw = Convert.FromBase64String(pemText);

                    rsa.ImportRSAPrivateKey(secretRaw, out _);

                    var ivBytes = IVSize / 8;

                    // Read and decrypt data encryption parameters
                    var buffer = new byte[256]; // OaepSHA256

                    await sourceStream.ReadAsync(buffer);

                    var ivAndKey = rsa.Decrypt(buffer, RSAEncryptionPadding.OaepSHA256);

                    using (var symmetric = CreateAlghorithm())
                    {
                        var iv = ivAndKey[0..ivBytes];
                        var key = ivAndKey[ivBytes..];

                        // Create an encryptor to perform the stream transform.
                        using (var decryptor = symmetric.CreateDecryptor(key, iv))
                        {
                            using (var cryptoStream = new CryptoStream(sourceStream, decryptor, CryptoStreamMode.Read, true))
                            {
                                await cryptoStream.CopyToAsync(targetStream);
                            }
                        }
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
            using (var rsa = new RSACng(KeySize))
            {
                var privateKey = rsa.ExportRSAPrivateKey();

                // Follow DER format specification the result is 270 bytes long (for key size 2048)
                // It consist of:
                // 1. collection of the following objects; that takes up 4 bytes
                // 2. The first object is an integer (which happens to be the public modulus);
                // the integer itself is 257 bytes (not 256; that's because ASN.1 integers are signed, and so there has to be a leading 00
                // to make the top bit zero to signify positive), as well as 4 overhead bytes, for a total of 261 bytes
                // 3. The second object is an integer (which happens to be the public exponent);
                // if you use 65537 as the exponent, this takes up a total of 5 bytes long (including overhead)
                // So 4+261+5 gives you a total of 270 bytes
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

        /// <summary>
        /// Symmetric algorith used in the hybrid crypto
        /// </summary>
        /// <returns></returns>
        protected virtual SymmetricAlgorithm CreateAlghorithm()
        {
            var aes = Aes.Create();

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC) and Padding PKCS7. Use default options for other symmetric key parameters.
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.BlockSize = 128;

            return aes;
        }

        protected virtual byte[] GenerateSymmetricKey(int bits)
        {
            using (var random = RandomNumberGenerator.Create())
            {
                var key = new byte[bits / 8];
                random.GetNonZeroBytes(key);

                return key;
            }
        }
    }
}
