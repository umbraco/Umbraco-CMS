using System;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
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
        public MemberPasswordHasher(LegacyPasswordSecurity legacyPasswordHasher, IJsonSerializer jsonSerializer)
            : base(legacyPasswordHasher, jsonSerializer)
        {
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

                throw new InvalidOperationException("unable to determine member password hashing algorith");
            }

            var isValid = LegacyPasswordSecurity.VerifyPassword(
                Constants.Security.AspNetUmbraco8PasswordHashAlgorithmName,
                providedPassword,
                hashedPassword);

            return isValid ? PasswordVerificationResult.SuccessRehashNeeded : PasswordVerificationResult.Failed;
        }
    }
}
