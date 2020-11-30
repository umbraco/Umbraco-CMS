using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Umbraco.Core.Members;

namespace Umbraco.Infrastructure.Members
{
    public interface IUmbracoMembersUserManager : IUmbracoMembersUserManager<UmbracoMembersIdentityUser>
    {
    }

    public interface IUmbracoMembersUserManager<TUser> : IDisposable where TUser : UmbracoMembersIdentityUser
    {
        /// <summary>
        /// Creates the specified <paramref name="memberUser"/> in the backing store with no password,
        /// as an asynchronous operation.
        /// </summary>
        /// <param name="memberUser">The member to create.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>
        /// of the operation.
        /// </returns>
        Task<IdentityResult> CreateAsync(TUser memberUser);

        /// <summary>
        /// Helper method to generate a password for a user based on the current password validator
        /// </summary>
        /// <returns></returns>
        string GeneratePassword();

        /// <summary>
        /// Adds the <paramref name="password"/> to the specified <paramref name="memberUser"/> only if the user
        /// does not already have a password.
        /// </summary>
        /// <param name="memberUser">The member whose password should be set.</param>
        /// <param name="password">The password to set.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>
        /// of the operation.
        /// </returns>
        Task<IdentityResult> AddPasswordAsync(TUser memberUser, string password);

        /// <summary>
        /// Returns a flag indicating whether the given <paramref name="password"/> is valid for the
        /// specified <paramref name="memberUser"/>.
        /// </summary>
        /// <param name="memberUser">The user whose password should be validated.</param>
        /// <param name="password">The password to validate</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing true if
        /// the specified <paramref name="password" /> matches the one store for the <paramref name="memberUser"/>,
        /// otherwise false.</returns>
        Task<bool> CheckPasswordAsync(TUser memberUser, string password);
    }
}
