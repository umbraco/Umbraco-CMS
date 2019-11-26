using System;
using System.Security.Cryptography;
using System.Text;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Security
{
    public class PasswordSecurity
    {
        private readonly IPasswordConfiguration _passwordConfiguration;

        public PasswordSecurity(IPasswordConfiguration passwordConfiguration)
        {
            _passwordConfiguration = passwordConfiguration;
        }

        public string HashPasswordForStorage(string password)
        {
            string salt;
            var hashed = EncryptOrHashNewPassword(password, out salt);
            return FormatPasswordForStorage(hashed, salt);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword)) throw new ArgumentException("Value cannot be null or whitespace.", "hashedPassword");
            return CheckPassword(password, hashedPassword);
        }

        /// <summary>
        /// If the password format is a hashed keyed algorithm then we will pre-pend the salt used to hash the password
        /// to the hashed password itself.
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public string FormatPasswordForStorage(string pass, string salt)
        {
            if (_passwordConfiguration.UseLegacyEncoding)
            {
                return pass;
            }

            return salt + pass;
        }

        public string EncryptOrHashPassword(string pass, string salt)
        {
            //if we are doing it the old way

            if (_passwordConfiguration.UseLegacyEncoding)
            {
                return LegacyEncodePassword(pass);
            }

            //This is the correct way to implement this (as per the sql membership provider)

            var bytes = Encoding.Unicode.GetBytes(pass);
            var saltBytes = Convert.FromBase64String(salt);
            byte[] inArray;

            var hashAlgorithm = GetHashAlgorithm(pass);
            var algorithm = hashAlgorithm as KeyedHashAlgorithm;
            if (algorithm != null)
            {
                var keyedHashAlgorithm = algorithm;
                if (keyedHashAlgorithm.Key.Length == saltBytes.Length)
                {
                    //if the salt bytes is the required key length for the algorithm, use it as-is
                    keyedHashAlgorithm.Key = saltBytes;
                }
                else if (keyedHashAlgorithm.Key.Length < saltBytes.Length)
                {
                    //if the salt bytes is too long for the required key length for the algorithm, reduce it
                    var numArray2 = new byte[keyedHashAlgorithm.Key.Length];
                    Buffer.BlockCopy(saltBytes, 0, numArray2, 0, numArray2.Length);
                    keyedHashAlgorithm.Key = numArray2;
                }
                else
                {
                    //if the salt bytes is too short for the required key length for the algorithm, extend it
                    var numArray2 = new byte[keyedHashAlgorithm.Key.Length];
                    var dstOffset = 0;
                    while (dstOffset < numArray2.Length)
                    {
                        var count = Math.Min(saltBytes.Length, numArray2.Length - dstOffset);
                        Buffer.BlockCopy(saltBytes, 0, numArray2, dstOffset, count);
                        dstOffset += count;
                    }
                    keyedHashAlgorithm.Key = numArray2;
                }
                inArray = keyedHashAlgorithm.ComputeHash(bytes);
            }
            else
            {
                var buffer = new byte[saltBytes.Length + bytes.Length];
                Buffer.BlockCopy(saltBytes, 0, buffer, 0, saltBytes.Length);
                Buffer.BlockCopy(bytes, 0, buffer, saltBytes.Length, bytes.Length);
                inArray = hashAlgorithm.ComputeHash(buffer);
            }

            return Convert.ToBase64String(inArray);
        }

        /// <summary>
        /// Checks the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="dbPassword">The dbPassword.</param>
        /// <returns></returns>
        public bool CheckPassword(string password, string dbPassword)
        {
            if (string.IsNullOrWhiteSpace(dbPassword)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(dbPassword));
            var storedHashedPass = StoredPassword(dbPassword, out var salt);
            var hashed = EncryptOrHashPassword(password, salt);
            return storedHashedPass == hashed;
        }

        /// <summary>
        /// Encrypt/hash a new password with a new salt
        /// </summary>
        /// <param name="newPassword"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public string EncryptOrHashNewPassword(string newPassword, out string salt)
        {
            salt = GenerateSalt();
            return EncryptOrHashPassword(newPassword, salt);
        }

        /// <summary>
        /// Returns the hashed password without the salt if it is hashed
        /// </summary>
        /// <param name="storedString"></param>
        /// <param name="salt">returns the salt</param>
        /// <returns></returns>
        public string StoredPassword(string storedString, out string salt)
        {
            if (string.IsNullOrWhiteSpace(storedString)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(storedString));
            if (_passwordConfiguration.UseLegacyEncoding)
            {
                salt = string.Empty;
                return storedString;
            }

            var saltLen = GenerateSalt();
            salt = storedString.Substring(0, saltLen.Length);
            return storedString.Substring(saltLen.Length);
        }

        public static string GenerateSalt()
        {
            var numArray = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(numArray);
            return Convert.ToBase64String(numArray);
        }

        public HashAlgorithm GetHashAlgorithm(string password)
        {
            if (_passwordConfiguration.UseLegacyEncoding)
            {
                return new HMACSHA1
                {
                    //the legacy salt was actually the password :(
                    Key = Encoding.Unicode.GetBytes(password)
                };
            }

            if (_passwordConfiguration.HashAlgorithmType.IsNullOrWhiteSpace())
                throw new InvalidOperationException("No hash algorithm type specified");

            var alg = HashAlgorithm.Create(_passwordConfiguration.HashAlgorithmType);
            if (alg == null)
                throw new InvalidOperationException($"The hash algorithm specified {_passwordConfiguration.HashAlgorithmType} cannot be resolved");

            return alg;
        }

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>The encoded password.</returns>
        private string LegacyEncodePassword(string password)
        {
            string encodedPassword = password;
            var hashAlgorith = GetHashAlgorithm(password);
            encodedPassword = Convert.ToBase64String(hashAlgorith.ComputeHash(Encoding.Unicode.GetBytes(password)));
            return encodedPassword;
        }

       

        

    }
}
