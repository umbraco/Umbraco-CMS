using System;
using Microsoft.AspNet.Identity;

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
        PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword);
    }
}