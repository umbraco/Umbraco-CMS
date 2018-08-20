using System;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// The default password hasher that is User aware so that it can process the hashing based on the user's settings
    /// </summary>
    public class UserAwareMembershipProviderPasswordHasher : MembershipProviderPasswordHasher, IUserAwarePasswordHasher<BackOfficeIdentityUser, int>
    {
        public UserAwareMembershipProviderPasswordHasher(MembershipProviderBase provider) : base(provider)
        {
        }

        public string HashPassword(BackOfficeIdentityUser user, string password)
        {
            //TODO: Implement the logic for this, we need to lookup the password format for the user and hash accordingly: http://issues.umbraco.org/issue/U4-10089
            //NOTE: For now this just falls back to the hashing we are currently using
            return base.HashPassword(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(BackOfficeIdentityUser user, string hashedPassword, string providedPassword)
        {
            //TODO: Implement the logic for this, we need to lookup the password format for the user and hash accordingly: http://issues.umbraco.org/issue/U4-10089
            //NOTE: For now this just falls back to the hashing we are currently using
            return base.VerifyHashedPassword(hashedPassword, providedPassword);
        }
    }
}