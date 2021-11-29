using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security
{
    /// <summary>
    /// A password hasher for members
    /// </summary>
    /// <remarks>
    /// This will check for the ASP.NET Identity password hash flag before falling back to the legacy password hashing format ("HMACSHA256")
    /// </remarks>
    public class MemberPasswordHasher : UmbracoPasswordHasher<MemberIdentityUser>
    {
        private readonly IOptions<LegacyMachineKeySettings> _legacyMachineKeySettings;

        [Obsolete("Use ctor with all params")]
        public MemberPasswordHasher(LegacyPasswordSecurity legacyPasswordHasher, IJsonSerializer jsonSerializer)
            : this(legacyPasswordHasher, jsonSerializer, StaticServiceProvider.Instance.GetRequiredService<IOptions<LegacyMachineKeySettings>>())
        {
        }

        public MemberPasswordHasher(LegacyPasswordSecurity legacyPasswordHasher, IJsonSerializer jsonSerializer, IOptions<LegacyMachineKeySettings> legacyMachineKeySettings)
            : base(legacyPasswordHasher, jsonSerializer)
        {
            _legacyMachineKeySettings = legacyMachineKeySettings;
        }
        /// <summary>
        /// Verifies a user's hashed password
        /// </summary>
        /// <param name="user"></param>
        /// <param name="hashedPassword"></param>
        /// <param name="providedPassword"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown when the correct hashing algorith cannot be determined</exception>
        public override PasswordVerificationResult VerifyHashedPassword(MemberIdentityUser user, string hashedPassword, string providedPassword)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // if there's password config use the base implementation
            if (!user.PasswordConfig.IsNullOrWhiteSpace())
            {
                return base.VerifyHashedPassword(user, hashedPassword, providedPassword);
            }

            // We need to check for clear text passwords from members as the first thing. This was possible in v8 :(
            if (IsSuccessfullLegacyPassword(hashedPassword, providedPassword))
            {
                return PasswordVerificationResult.SuccessRehashNeeded;
            }

            // Else we need to detect what the password is. This will be the case
            // for upgrades since no password config will exist.

            byte[] decodedHashedPassword = null;
            bool isAspNetIdentityHash = false;

            try
            {
                decodedHashedPassword = Convert.FromBase64String(hashedPassword);
                isAspNetIdentityHash = true;
            }
            catch (Exception)
            {
                // ignored - decoding throws
            }

            // check for default ASP.NET Identity password hash flags
            if (isAspNetIdentityHash)
            {
                if (decodedHashedPassword[0] == 0x00 || decodedHashedPassword[0] == 0x01)
                {
                    return base.VerifyHashedPassword(user, hashedPassword, providedPassword);
                }

                throw new InvalidOperationException("unable to determine member password hashing algorithm");
            }



            var isValid = LegacyPasswordSecurity.VerifyPassword(
                Constants.Security.AspNetUmbraco8PasswordHashAlgorithmName,
                providedPassword,
                hashedPassword);

            return isValid ? PasswordVerificationResult.SuccessRehashNeeded : PasswordVerificationResult.Failed;
        }

        private bool IsSuccessfullLegacyPassword(string hashedPassword, string providedPassword)
        {
            if (hashedPassword == providedPassword)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(_legacyMachineKeySettings.Value.DecryptionKey))
            {
                try
                {
                    var decryptedPassword = DecryptLegacyPassword(hashedPassword, _legacyMachineKeySettings.Value.Decryption,  _legacyMachineKeySettings.Value.DecryptionKey);
                    return decryptedPassword == providedPassword;
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException(
                        "Could not decrypt password even that a DecryptionKey is provided. This means the DecryptionKey is wrong.", ex);
                }
            }

            var result = LegacyPasswordSecurity.VerifyPassword(Constants.Security.AspNetUmbraco8PasswordHashAlgorithmName, providedPassword, hashedPassword);

            if (result)
            {
                return result;
            }
            return LegacyPasswordSecurity.VerifyPassword(Constants.Security.AspNetUmbraco4PasswordHashAlgorithmName, providedPassword, hashedPassword);

        }

        private static string DecryptLegacyPassword(string encryptedPassword, string algorithmName, string decryptionKey)
        {
            SymmetricAlgorithm algorithm;
            switch (algorithmName)
            {
                case "AES":
                    algorithm = new AesCryptoServiceProvider()
                    {
                        Key = StringToByteArray(decryptionKey),
                        IV = new byte[16]
                    };
                    break;
                default:
                    throw new NotSupportedException($"The algorithm ({algorithmName}) is not supported");
            }

            using (algorithm)
            {
                return DecryptLegacyPassword(encryptedPassword, algorithm);
            }
        }

        private static string DecryptLegacyPassword(string encryptedPassword, SymmetricAlgorithm algorithm)
        {
            using var memoryStream = new MemoryStream();
            ICryptoTransform cryptoTransform = algorithm.CreateDecryptor();
            var cryptoStream = new CryptoStream((Stream)memoryStream, cryptoTransform, CryptoStreamMode.Write);
            var buf = Convert.FromBase64String(encryptedPassword);
            cryptoStream.Write(buf, 0, 32);
            cryptoStream.FlushFinalBlock();

            return Encoding.Unicode.GetString(memoryStream.ToArray());
        }

        private static byte[] StringToByteArray(string hex) =>
            Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
    }
}
