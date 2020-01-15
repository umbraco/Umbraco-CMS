using System;
using Microsoft.AspNet.Identity;
using Umbraco.Web.Models.Identity;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// The default password hasher that is User aware so that it can process the hashing based on the user's settings
    /// </summary>
    public class UserAwarePasswordHasher : IUserAwarePasswordHasher<BackOfficeIdentityUser, int>
    {
        private readonly PasswordSecurity _passwordSecurity;

        public UserAwarePasswordHasher(PasswordSecurity passwordSecurity)
        {
            _passwordSecurity = passwordSecurity;
        }

        public string HashPassword(string password)
        {
            return _passwordSecurity.HashPasswordForStorage(password);
        }

        public string HashPassword(BackOfficeIdentityUser user, string password)
        {
            // TODO: Implement the logic for this, we need to lookup the password format for the user and hash accordingly: http://issues.umbraco.org/issue/U4-10089
            //NOTE: For now this just falls back to the hashing we are currently using

            return HashPassword(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            return _passwordSecurity.VerifyPassword(providedPassword, hashedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }

        public PasswordVerificationResult VerifyHashedPassword(BackOfficeIdentityUser user, string hashedPassword, string providedPassword)
        {
            // TODO: Implement the logic for this, we need to lookup the password format for the user and hash accordingly: http://issues.umbraco.org/issue/U4-10089
            //NOTE: For now this just falls back to the hashing we are currently using

            return VerifyHashedPassword(hashedPassword, providedPassword);
        }
    }
}
