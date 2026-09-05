using System.Security.Cryptography;
using System.Text;
using appointmentapi.Services.Interface;
using appointmentapi.Settings;
using Microsoft.Extensions.Options;

namespace appointmentapi.Services
{
    public class CryptoService : ICryptoService
    {
        private readonly byte[] _chave;

        public CryptoService(IOptions<EncryptionSettings> settings)
        {
            _chave = Encoding.UTF8.GetBytes(settings.Value.MasterKey);
        }

        public (string cifrado, string iv) Criptografar(string textoPuro)
        {
            using var aes = Aes.Create();
            aes.Key = _chave;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var bytesTexto = Encoding.UTF8.GetBytes(textoPuro);
            var bytesCifrados = encryptor.TransformFinalBlock(bytesTexto, 0, bytesTexto.Length);

            return (Convert.ToBase64String(bytesCifrados), Convert.ToBase64String(aes.IV));
        }

        public string Descriptografar(string cifrado, string iv)
        {
            using var aes = Aes.Create();
            aes.Key = _chave;
            aes.IV = Convert.FromBase64String(iv);

            using var decryptor = aes.CreateDecryptor();
            var bytesCifrados = Convert.FromBase64String(cifrado);
            var bytesTexto = decryptor.TransformFinalBlock(bytesCifrados, 0, bytesCifrados.Length);

            return Encoding.UTF8.GetString(bytesTexto);
        }
    }
}