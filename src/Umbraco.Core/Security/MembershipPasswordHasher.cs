using Microsoft.AspNet.Identity;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// A custom password hasher that conforms to the current password hashing done in Umbraco
    /// </summary>
    internal class MembershipPasswordHasher : IPasswordHasher
    {
        private readonly MembershipProviderBase _provider;

        public MembershipPasswordHasher(MembershipProviderBase provider)
        {
            _provider = provider;
        }

        public string HashPassword(string password)
        {
            return _provider.HashPasswordForStorage(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            return _provider.VerifyPassword(providedPassword, hashedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }


    }
}