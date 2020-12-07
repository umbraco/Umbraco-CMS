using Microsoft.AspNetCore.Identity;
using Umbraco.Core.Security;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Microsoft.Extensions.Options;
using Umbraco.Core.Serialization;

namespace Umbraco.Web.BackOffice.Security
{
    /// <summary>
    /// A password hasher for back office users
    /// </summary>
    /// <remarks>
    /// This allows us to verify passwords in old formats and roll forward to the latest format
    /// </remarks>
    public class BackOfficePasswordHasher : PasswordHasher<BackOfficeIdentityUser>
    {
        private readonly LegacyPasswordSecurity _legacyPasswordSecurity;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly PasswordHasher<BackOfficeIdentityUser> _aspnetV2PasswordHasher = new PasswordHasher<BackOfficeIdentityUser>(new V2PasswordHasherOptions());

        public BackOfficePasswordHasher(LegacyPasswordSecurity passwordSecurity, IJsonSerializer jsonSerializer)
        {
            _legacyPasswordSecurity = passwordSecurity;
            _jsonSerializer = jsonSerializer;
        }

        public override string HashPassword(BackOfficeIdentityUser user, string password)
        {
            // Always use the latest/current hash algorithm when hashing new passwords for storage.
            // NOTE: This is only overridden to show that we can since we may need to adjust this in the future
            // if new/different formats are required.
            return base.HashPassword(user, password);
        }

        /// <summary>
        /// Verifies a user's hashed password
        /// </summary>
        /// <param name="user"></param>
        /// <param name="hashedPassword"></param>
        /// <param name="providedPassword"></param>
        /// <returns></returns>
        /// <remarks>
        /// This will check the user's current hashed password format stored with their user row and use that to verify the hash. This could be any hashes
        /// from the very old v4, to the older v6-v8, to the older aspnet identity and finally to the most recent 
        /// </remarks>
        public override PasswordVerificationResult VerifyHashedPassword(BackOfficeIdentityUser user, string hashedPassword, string providedPassword)
        {
            if (!user.PasswordConfig.IsNullOrWhiteSpace())
            {
                // check if the (legacy) password security supports this hash algorith and if so then use it
                var deserialized = _jsonSerializer.Deserialize<UserPasswordSettings>(user.PasswordConfig);
                if (_legacyPasswordSecurity.SupportHashAlgorithm(deserialized.HashAlgorithm))
                {
                    var result = _legacyPasswordSecurity.VerifyPassword(deserialized.HashAlgorithm, providedPassword, hashedPassword);
                    return result
                        ? PasswordVerificationResult.SuccessRehashNeeded
                        : PasswordVerificationResult.Failed;
                }

                // We will explicitly detect names here
                // The default is PBKDF2.ASPNETCORE.V3:
                //      PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey, 10000 iterations.                
                // The underlying class only lets us change 2 things which is the version: options.CompatibilityMode and the iteration count
                // The PBKDF2.ASPNETCORE.V2 settings are:
                //      PBKDF2 with HMAC-SHA1, 128-bit salt, 256-bit subkey, 1000 iterations.                

                switch (deserialized.HashAlgorithm)
                {
                    case Constants.Security.AspNetCoreV3PasswordHashAlgorithmName:
                        return base.VerifyHashedPassword(user, hashedPassword, providedPassword);
                    case Constants.Security.AspNetCoreV2PasswordHashAlgorithmName:
                        var legacyResult = _aspnetV2PasswordHasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
                        if (legacyResult == PasswordVerificationResult.Success) return PasswordVerificationResult.SuccessRehashNeeded;
                        return legacyResult;
                }
            }

            // else go the default (v3)
            return base.VerifyHashedPassword(user, hashedPassword, providedPassword);
        }

        private class V2PasswordHasherOptions : IOptions<PasswordHasherOptions>
        {
            public PasswordHasherOptions Value => new PasswordHasherOptions
            {
                CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2
            };
        }
    }
}
