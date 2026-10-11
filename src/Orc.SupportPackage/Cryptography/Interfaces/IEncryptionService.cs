namespace Orc.SupportPackage
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IEncryptionService
    {
        Task DecryptAsync(Stream sourceStream, Stream targetStream, EncryptionContext content);
        Task EncryptAsync(Stream sourceStream, Stream targetStream, EncryptionContext content);

        public void Generate(string secretLocation, string publicKeyLocation);
        Task<string> ReadPublicKeyFromPemFileAsync(string keyPath);
    }
}
