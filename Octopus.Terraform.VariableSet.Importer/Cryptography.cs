using System.Security.Cryptography;
using System.Text;

namespace OctopusTenantCreator.Common
{
    public class Cryptography
    {
        private string? _masterKey { get; set; }
        public Cryptography(string? masterKey)
        {
            _masterKey = masterKey;
        }
        
        public string DecryptSensitiveVariable (string encodedValue)
        {
            if (string.IsNullOrWhiteSpace(_masterKey)) return encodedValue;
            if (string.IsNullOrWhiteSpace(encodedValue)) return encodedValue;
            var parts = encodedValue.Split('|');

            if (parts.Length != 2) return encodedValue;

            var cipher = Convert.FromBase64String(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);

            var key = Convert.FromBase64String(_masterKey);

            using (var algorithm = new AesCryptoServiceProvider
            {
                Padding = PaddingMode.PKCS7,
                KeySize = 128,
                Key = key,
                BlockSize = 128,
                Mode = CipherMode.CBC,
                IV = salt
            })

            using (var memoryStream = new MemoryStream())
            {
                using (var decryptor = algorithm.CreateDecryptor())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(cipher, 0, cipher.Length);
                        cryptoStream.FlushFinalBlock();
                        var s = Encoding.UTF8.GetString(memoryStream.ToArray());
                        return s;
                    }
                }
            }
        }
    }
}