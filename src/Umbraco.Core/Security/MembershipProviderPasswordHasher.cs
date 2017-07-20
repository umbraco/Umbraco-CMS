using System;
using Microsoft.AspNet.Identity;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// A password hasher that is User aware so that it can process the hashing based on the user's settings
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TUser"></typeparam>
    public interface IUserAwarePasswordHasher<in TUser, TKey>
        where TUser : class, IUser<TKey>
        where TKey : IEquatable<TKey>
    {
        string HashPassword(TUser user, string password);
        string VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword);
    }

    /// <summary>
    /// A password hasher that is based on the rules configured for a membership provider
    /// </summary>
    public interface IMembershipProviderPasswordHasher : IPasswordHasher
    {
        MembershipProviderBase MembershipProvider { get; }
    }

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
            throw new NotImplementedException();
        }

        public string VerifyHashedPassword(BackOfficeIdentityUser user, string hashedPassword, string providedPassword)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A password hasher that conforms to the password hashing done with membership providers
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