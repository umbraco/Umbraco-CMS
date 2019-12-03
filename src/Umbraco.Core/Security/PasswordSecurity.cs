using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// Handles password hashing and formatting
    /// </summary>
    public class PasswordSecurity
    {
        public IPasswordConfiguration PasswordConfiguration { get; }
        public PasswordGenerator _generator;
        public ConfiguredPasswordValidator _validator;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="passwordConfiguration"></param>
        public PasswordSecurity(IPasswordConfiguration passwordConfiguration)
        {
            PasswordConfiguration = passwordConfiguration;
        }

        /// <summary>
        /// Checks if the password passes validation rules
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<Attempt<IEnumerable<string>>> IsValidPasswordAsync(string password)
        {
            if (_validator == null)
                _validator = new ConfiguredPasswordValidator(PasswordConfiguration);
            var result = await _validator.ValidateAsync(password);
            if (result.Succeeded)
                return Attempt<IEnumerable<string>>.Succeed();

            return Attempt<IEnumerable<string>>.Fail(result.Errors);
        }

        public string GeneratePassword()
        {
            if (_generator == null)
                _generator = new PasswordGenerator(PasswordConfiguration);
            return _generator.GeneratePassword();
        }

        /// <summary>
        /// Returns a hashed password value used to store in a data store
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string HashPasswordForStorage(string password)
        {
            string salt;
            var hashed = HashNewPassword(password, out salt);
            return FormatPasswordForStorage(hashed, salt);
        }

        /// <summary>
        /// If the password format is a hashed keyed algorithm then we will pre-pend the salt used to hash the password
        /// to the hashed password itself.
        /// </summary>
        /// <param name="hashedPassword"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public string FormatPasswordForStorage(string hashedPassword, string salt)
        {
            if (PasswordConfiguration.UseLegacyEncoding)
            {
                return hashedPassword;
            }

            return salt + hashedPassword;
        }

        /// <summary>
        /// Hashes a password with a given salt
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public string HashPassword(string pass, string salt)
        {
            //if we are doing it the old way

            if (PasswordConfiguration.UseLegacyEncoding)
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
        /// Verifies if the password matches the expected hash+salt of the stored password string
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="dbPassword">The value of the password stored in a data store.</param>
        /// <returns></returns>
        public bool VerifyPassword(string password, string dbPassword)
        {
            if (string.IsNullOrWhiteSpace(dbPassword)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(dbPassword));

            if (dbPassword.StartsWith(Constants.Security.EmptyPasswordPrefix))
                return false;

            var storedHashedPass = ParseStoredHashPassword(dbPassword, out var salt);
            var hashed = HashPassword(password, salt);
            return storedHashedPass == hashed;
        }

        /// <summary>
        /// Create a new password hash and a new salt
        /// </summary>
        /// <param name="newPassword"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public string HashNewPassword(string newPassword, out string salt)
        {
            salt = GenerateSalt();
            return HashPassword(newPassword, salt);
        }

        /// <summary>
        /// Parses out the hashed password and the salt from the stored password string value
        /// </summary>
        /// <param name="storedString"></param>
        /// <param name="salt">returns the salt</param>
        /// <returns></returns>
        public string ParseStoredHashPassword(string storedString, out string salt)
        {
            if (string.IsNullOrWhiteSpace(storedString)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(storedString));
            if (PasswordConfiguration.UseLegacyEncoding)
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

        /// <summary>
        /// Return the hash algorithm to use
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public HashAlgorithm GetHashAlgorithm(string password)
        {
            if (PasswordConfiguration.UseLegacyEncoding)
            {
                return new HMACSHA1
                {
                    //the legacy salt was actually the password :(
                    Key = Encoding.Unicode.GetBytes(password)
                };
            }

            if (PasswordConfiguration.HashAlgorithmType.IsNullOrWhiteSpace())
                throw new InvalidOperationException("No hash algorithm type specified");

            var alg = HashAlgorithm.Create(PasswordConfiguration.HashAlgorithmType);
            if (alg == null)
                throw new InvalidOperationException($"The hash algorithm specified {PasswordConfiguration.HashAlgorithmType} cannot be resolved");

            return alg;
        }

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>The encoded password.</returns>
        private string LegacyEncodePassword(string password)
        {
            var hashAlgorith = GetHashAlgorithm(password);
            var encodedPassword = Convert.ToBase64String(hashAlgorith.ComputeHash(Encoding.Unicode.GetBytes(password)));
            return encodedPassword;
        }

       

        

    }
}
