using System;
using Microsoft.AspNet.Identity;

namespace Umbraco.Core.Security
{
    public interface IUserAwarePasswordHasher<TKey>
        where TKey : IEquatable<TKey>
    {
        string HashPassword(TKey id, string password);
        string VerifyHashedPassword(TKey id, string hashedPassword, string providedPassword);
    }

    public interface IMembershipProviderPasswordHasher : IPasswordHasher
    {
        MembershipProviderBase MembershipProvider { get; }
    }

    /// <summary>
    /// A custom password hasher that conforms to the password hashing done with membership providers
    /// </summary>
    public class MembershipProviderPasswordHasher : IMembershipProviderPasswordHasher
    {
        /// <summary>
        /// Exposes the underlying MembershipProvider
        /// </summary>
        public MembershipProviderBase MembershipProvider { get; private set; }

        public MembershipProviderPasswordHasher(MembershipProviderBase provider)
        {
            MembershipProvider = provider;
        }

        public string HashPassword(string password)
        {
            return MembershipProvider.HashPasswordForStorage(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            return MembershipProvider.VerifyPassword(providedPassword, hashedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }


    }
}