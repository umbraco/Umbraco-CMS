using Microsoft.AspNetCore.Identity;
using Umbraco.Core.BackOffice;
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
    public class BackOfficePasswordHasher : PasswordHasher<BackOfficeIdentityUser>
    {
        private readonly LegacyPasswordSecurity _legacyPasswordSecurity;
        private readonly IJsonSerializer _jsonSerializer;

        public BackOfficePasswordHasher(LegacyPasswordSecurity passwordSecurity, IJsonSerializer jsonSerializer)
        {
            _legacyPasswordSecurity = passwordSecurity;
            _jsonSerializer = jsonSerializer;
        }

        public override string HashPassword(BackOfficeIdentityUser user, string password)
        {
            if (!user.PasswordConfig.IsNullOrWhiteSpace())
            {
                // check if the (legacy) password security supports this hash algorith and if so then use it
                // TODO: I don't think we really want to do this because we want to migrate all old password formats
                // to the new format, not keep using it. AFAIK this block should be removed along with the code within the
                // LegacyPasswordSecurity except the part that just validates old ones.
                var deserialized = _jsonSerializer.Deserialize<UserPasswordSettings>(user.PasswordConfig);
                if (_legacyPasswordSecurity.SupportHashAlgorithm(deserialized.HashAlgorithm))
                    return _legacyPasswordSecurity.HashPasswordForStorage(password);

                // We will explicitly detect names here since this allows us to future proof these checks.
                // The default is PBKDF2.ASPNETCORE.V3:
                //      PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey, 10000 iterations.                
                // The underlying class only lets us change 2 things which is the version: options.CompatibilityMode and the iteration count
                // The PBKDF2.ASPNETCORE.V2 settings are:
                //      PBKDF2 with HMAC-SHA1, 128-bit salt, 256-bit subkey, 1000 iterations.                

                switch (deserialized.HashAlgorithm)
                {
                    case Constants.Security.AspNetCoreV3PasswordHashAlgorithmName:
                        return base.HashPassword(user, password);
                    case Constants.Security.AspNetCoreV2PasswordHashAlgorithmName:
                        var v2Hasher = new PasswordHasher<BackOfficeIdentityUser>(new V2PasswordHasherOptions());
                        return v2Hasher.HashPassword(user, password);
                }
            }

            // else keep the default
            return base.HashPassword(user, password);
        }

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
                        ? PasswordVerificationResult.Success
                        : PasswordVerificationResult.Failed;
                }

                switch (deserialized.HashAlgorithm)
                {
                    case Constants.Security.AspNetCoreV3PasswordHashAlgorithmName:
                        return base.VerifyHashedPassword(user, hashedPassword, providedPassword);
                    case Constants.Security.AspNetCoreV2PasswordHashAlgorithmName:
                        var v2Hasher = new PasswordHasher<BackOfficeIdentityUser>(new V2PasswordHasherOptions());
                        return v2Hasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
                }
            }

            // else go the default
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
