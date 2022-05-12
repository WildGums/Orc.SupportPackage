namespace Orc.SupportPackage
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Logging;

    public class EncryptionService : IEncryptionService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private const int PasswordIterations = 2071;
        private const int KeySize = 256;

        private const string SaltValue = "5Sr>3rCxUTf@3~Uu";
        private const string InitVector = "fe,r7<mFXRD?wR57"; // must be 16 bytes

        public async Task EncryptAsync(
            Stream sourceStream,
            Stream targetStream,
            EncryptionContext content)
        {
            try
            {
                Argument.IsNotNullOrEmpty(() => content.PassPhrase);

                // Convert strings into byte arrays.
                // Let us assume that strings only contain ASCII codes.
                // If strings include Unicode characters, use Unicode, UTF7, or UTF8 encoding.
                var initVectorBytes = Encoding.UTF8.GetBytes(content.InitVector ?? InitVector);
                var saltValueBytes = Encoding.UTF8.GetBytes(content.SaltValue ?? SaltValue);

                // First, we must create a password, from which the key will be derived.
                // This password will be generated from the specified passphrase and 
                // salt value. The password will be created using the specified hash 
                // algorithm. Password creation can be done in several iterations.
                using (var password = new Rfc2898DeriveBytes(content.PassPhrase, saltValueBytes, content.PasswordIterations ?? PasswordIterations))
                {
                    using (var hmac = new HMACSHA256(GetRawKey(content)))
                    {
                        // Use the password to generate pseudo-random bytes for the encryption
                        // key. Specify the size of the key in bytes (instead of bits).
                        var keyBytes = password.GetBytes(Convert.ToInt32((content.KeySize ?? KeySize) / 8));
                        var hashBytesSize = hmac.HashSize / 8;

                        using (var symmetricKey = CreateAlghorithm())
                        {
                            // Generate encryptor from the existing key bytes and initialization 
                            // vector. Key size will be defined based on the number of the key 
                            // bytes.
                            using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes))
                            {
                                // Define cryptographic stream (always use Write mode for encryption)
                                using (var cryptoStream = new CryptoStream(targetStream, encryptor, CryptoStreamMode.Write, true))
                                {
                                    targetStream.Seek(hashBytesSize, SeekOrigin.Begin);

                                    await sourceStream.CopyToAsync(cryptoStream, 4096);
                                    await cryptoStream.FlushAsync();

                                    // Finish encrypting
#pragma warning disable CL0001 // Use async overload inside this async method
                                    cryptoStream.FlushFinalBlock();
#pragma warning restore CL0001 // Use async overload inside this async method

                                    await targetStream.FlushAsync();
                                }
                            }

                            targetStream.Seek(hashBytesSize, SeekOrigin.Begin);
                            var hmacbytes = hmac.ComputeHash(targetStream);

                            targetStream.Seek(0, SeekOrigin.Begin);
                            targetStream.Write(hmacbytes, 0, hmacbytes.Length); // Always 32 bytes

                            await targetStream.FlushAsync();
                        }
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
            EncryptionContext content)
        {
            try
            {
                Argument.IsNotNullOrEmpty(() => content.PassPhrase);

                // Convert strings defining encryption key characteristics into byte
                // arrays. Let us assume that strings only contain ASCII codes.
                // If strings include Unicode characters, use Unicode, UTF7, or UTF8
                // encoding.
                var initVectorBytes = Encoding.UTF8.GetBytes(content.InitVector ?? InitVector);
                var saltValueBytes = Encoding.UTF8.GetBytes(content.SaltValue ?? SaltValue);

                var canDecrypt = await VerifyAsync(sourceStream, content);
                if (!canDecrypt)
                {
                    throw Log.ErrorAndCreateException<CryptographicException>("Cannot decrypt source with provided secret");
                }

                // Then, we must create a password, from which the key will be 
                // derived. This password will be generated from the specified 
                // passphrase and salt value. The password will be created using
                // the specified hash algorithm. Password creation can be done in
                // several iterations.
                using (var password = new Rfc2898DeriveBytes(content.PassPhrase, saltValueBytes, content.PasswordIterations ?? PasswordIterations))
                {
                    // Use the password to generate pseudo-random bytes for the encryption
                    // key. Specify the size of the key in bytes (instead of bits).
                    var keyBytes = password.GetBytes(Convert.ToInt32((content.KeySize ?? KeySize) / 8));

                    using (var symmetricKey = CreateAlghorithm())
                    {
                        // Generate decryptor from the existing key bytes and initialization 
                        // vector. Key size will be defined based on the number of the key 
                        // bytes.
                        using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes))
                        {
                            // Define memory stream which will be used to hold encrypted data.

                            using (var hmac = new HMACSHA256(GetRawKey(content)))
                            {
                                // Skip the checksum bytes
                                sourceStream.Position = sourceStream.Position + (hmac.HashSize / 8);
                            }

                            using (var cryptoStream = new CryptoStream(sourceStream, decryptor, CryptoStreamMode.Read, true))
                            {
                                // Decrypt
                                var position = targetStream.Position;

                                await cryptoStream.CopyToAsync(targetStream, 4096);
                                await targetStream.FlushAsync();

                                targetStream.Position = position;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to decrypt");
                throw;
            }
        }

        public async Task<bool> VerifyAsync(Stream sourceStream, EncryptionContext encryptionContext)
        {
            Argument.IsNotNull(() => encryptionContext);

            var error = false;

            var rawKey = GetRawKey(encryptionContext);

            // First, we must compare the key in the source with a new key created for the data portion of the file.
            // Read hash and compute the hash of the remaining contents of the file.
            // If the keys are not equals, then parameter key not valid or data has been tampered with.
            using (var hmac = new HMACSHA256(rawKey))
            {
                var storedHash = new byte[hmac.HashSize / 8];

                var sourceStreamPosition = sourceStream.Position;

                await sourceStream.ReadAsync(storedHash, 0, storedHash.Length);

                // TODO: Async hashing not supported in NET Core 3.1
                var computedHash = hmac.ComputeHash(sourceStream);

                for (var i = 0; i < storedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i])
                    {
                        error = true;
                        break;
                    }
                }

                sourceStream.Position = sourceStreamPosition;
            }

            return !error;
        }

        private byte[] GetRawKey(EncryptionContext context)
        {
            Argument.IsNotNullOrEmpty(() => context.PassPhrase);
            var rawKey = Encoding.UTF8.GetBytes(context.PassPhrase);
            return rawKey;
        }

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
    }
}
