using System;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Cms.Infrastructure.Security
{
    /// <summary>
    /// A password hasher for members
    /// </summary>
    /// <remarks>
    /// This will check for the ASP.NET Identity password hash flag before falling back to the legacy password hashing format ("HMACSHA256")
    /// </remarks>
    public class MemberPasswordHasher : PasswordHasher<MembersIdentityUser>
    {
        private readonly LegacyPasswordSecurity _legacyPasswordHasher;

        public MemberPasswordHasher(LegacyPasswordSecurity legacyPasswordHasher)
        {
            _legacyPasswordHasher = legacyPasswordHasher ?? throw new ArgumentNullException(nameof(legacyPasswordHasher));
        }

        /// <summary>
        /// Verifies a user's hashed password
        /// </summary>
        /// <param name="user"></param>
        /// <param name="hashedPassword"></param>
        /// <param name="providedPassword"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown when the correct hashing algorith cannot be determined</exception>
        public override PasswordVerificationResult VerifyHashedPassword(MembersIdentityUser user, string hashedPassword, string providedPassword)
        {
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
                    return base.VerifyHashedPassword(user, hashedPassword, providedPassword);

                throw new InvalidOperationException("unable to determine member password hashing algorith");
            }

            var isValid = _legacyPasswordHasher.VerifyPassword(
                Constants.Security.AspNetUmbraco8PasswordHashAlgorithmName,
                providedPassword,
                hashedPassword);

            return isValid ? PasswordVerificationResult.SuccessRehashNeeded : PasswordVerificationResult.Failed;
        }
    }
}
