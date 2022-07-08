using System.Security.Cryptography;
using System.Text;
using Umbraco.Cms.Core.OAuth;

namespace Umbraco.Cms.Infrastructure.OAuth
{
    // Hat-tip: https://stackoverflow.com/a/51947250/489433
    internal class SecretEncryptor : ISecretEncryptor
    {
        private readonly string _secretKey;

        public SecretEncryptor(string secretKey) => _secretKey = secretKey;

        public string Encrypt(string value)
        {
            if (string.IsNullOrEmpty(_secretKey))
            {
                throw new ArgumentException("No secret key has been configured.", nameof(_secretKey));
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("The value to be encrypted cannot be empty.", nameof(value));
            }

            var buffer = Encoding.UTF8.GetBytes(value);
            var hash = new SHA512CryptoServiceProvider();
            var aesKey = new byte[24];
            Buffer.BlockCopy(hash.ComputeHash(Encoding.UTF8.GetBytes(_secretKey)), 0, aesKey, 0, 24);

            using (var aes = Aes.Create())
            {
                aes.Key = aesKey;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, encryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(buffer))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    var result = resultStream.ToArray();
                    var combined = new byte[aes.IV.Length + result.Length];
                    Array.ConstrainedCopy(aes.IV, 0, combined, 0, aes.IV.Length);
                    Array.ConstrainedCopy(result, 0, combined, aes.IV.Length, result.Length);

                    return Convert.ToBase64String(combined);
                }
            }
        }

        public string Decrypt(string value)
        {
            if (string.IsNullOrEmpty(_secretKey))
            {
                throw new ArgumentException("No secret key has been configured.", nameof(_secretKey));
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("The value to be decrypted cannot be empty.", nameof(value));
            }

            var combined = Convert.FromBase64String(value);
            var buffer = new byte[combined.Length];
            var hash = new SHA512CryptoServiceProvider();
            var aesKey = new byte[24];
            Buffer.BlockCopy(hash.ComputeHash(Encoding.UTF8.GetBytes(_secretKey)), 0, aesKey, 0, 24);

            using (var aes = Aes.Create())
            {
                aes.Key = aesKey;

                var iv = new byte[aes.IV.Length];
                var ciphertext = new byte[buffer.Length - iv.Length];

                Array.ConstrainedCopy(combined, 0, iv, 0, iv.Length);
                Array.ConstrainedCopy(combined, iv.Length, ciphertext, 0, ciphertext.Length);

                aes.IV = iv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var resultStream = new MemoryStream())
                {
                    using (var aesStream = new CryptoStream(resultStream, decryptor, CryptoStreamMode.Write))
                    using (var plainStream = new MemoryStream(ciphertext))
                    {
                        plainStream.CopyTo(aesStream);
                    }

                    return Encoding.UTF8.GetString(resultStream.ToArray());
                }
            }
        }
    }
}
