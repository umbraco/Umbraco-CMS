using Microsoft.AspNetCore.Identity;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Security;

namespace Umbraco.Web.Security
{
    public class UserAwarePasswordHasher<T> : IPasswordHasher<T>
        where T : BackOfficeIdentityUser
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

        public string HashPassword(T user, string password)
        {
            // TODO: Implement the logic for this, we need to lookup the password format for the user and hash accordingly: http://issues.umbraco.org/issue/U4-10089
            //NOTE: For now this just falls back to the hashing we are currently using

            return HashPassword(password);
        }
        
        public PasswordVerificationResult VerifyHashedPassword(T user, string hashedPassword, string providedPassword)
        {
            // TODO: Implement the logic for this, we need to lookup the password format for the user and hash accordingly: http://issues.umbraco.org/issue/U4-10089
            //NOTE: For now this just falls back to the hashing we are currently using

            return _passwordSecurity.VerifyPassword(providedPassword, hashedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
    }
}
