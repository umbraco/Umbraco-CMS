using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Models.Identity;
using Umbraco.Cms.Core.Net;

namespace Umbraco.Cms.Core.Security
{
    /// <summary>
    /// Abstract class for Umbraco User Managers for back office users or front-end members
    /// </summary>
    /// <typeparam name="TUser">The type of user</typeparam>
    /// /// <typeparam name="TPasswordConfig">The type password config</typeparam>
    public abstract class UmbracoUserManager<TUser, TPasswordConfig> : UserManager<TUser>
        where TUser : UmbracoIdentityUser
        where TPasswordConfig : class, IPasswordConfiguration, new()
    {
        private PasswordGenerator _passwordGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoUserManager{T, TPasswordConfig}"/> class.
        /// </summary>
        public UmbracoUserManager(
            IIpResolver ipResolver,
            IUserStore<TUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<TUser> passwordHasher,
            IEnumerable<IUserValidator<TUser>> userValidators,
            IEnumerable<IPasswordValidator<TUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<TUser>> logger,
            IOptions<TPasswordConfig> passwordConfiguration)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            IpResolver = ipResolver ?? throw new ArgumentNullException(nameof(ipResolver));
            PasswordConfiguration = passwordConfiguration.Value ?? throw new ArgumentNullException(nameof(passwordConfiguration));
        }

        /// <inheritdoc />
        public override bool SupportsUserClaim => false; // We don't support an IUserClaimStore and don't need to (at least currently)

        /// <inheritdoc />
        public override bool SupportsQueryableUsers => false; // It would be nice to support this but we don't need to currently and that would require IQueryable support for our user service/repository

        /// <summary>
        /// Developers will need to override this to support custom 2 factor auth
        /// </summary>
        /// <inheritdoc />
        public override bool SupportsUserTwoFactor => false;

        /// <inheritdoc />
        public override bool SupportsUserPhoneNumber => false; // We haven't needed to support this yet, though might be necessary for 2FA

        /// <summary>
        /// Gets the password configuration
        /// </summary>
        public IPasswordConfiguration PasswordConfiguration { get; }

        /// <summary>
        /// Gets the IP resolver
        /// </summary>
        public IIpResolver IpResolver { get; }

        /// <summary>
        /// Used to validate a user's session
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <param name="sessionId">The sesion id</param>
        /// <returns>True if the sesion is valid, else false</returns>
        public virtual async Task<bool> ValidateSessionIdAsync(string userId, string sessionId)
        {
            var userSessionStore = Store as IUserSessionStore<TUser>;

            // if this is not set, for backwards compat (which would be super rare), we'll just approve it
            // TODO: This should be removed after members supports this
            if (userSessionStore == null)
            {
                return true;
            }

            return await userSessionStore.ValidateSessionIdAsync(userId, sessionId);
        }

        /// <summary>
        /// This will determine which password hasher to use based on what is defined in config
        /// </summary>
        /// <param name="passwordConfiguration">The <see cref="IPasswordConfiguration"/></param>
        /// <returns>An <see cref="IPasswordHasher{T}"/></returns>
        protected virtual IPasswordHasher<TUser> GetDefaultPasswordHasher(IPasswordConfiguration passwordConfiguration) => new PasswordHasher<TUser>();

        /// <summary>
        /// Helper method to generate a password for a user based on the current password validator
        /// </summary>
        /// <returns>The generated password</returns>
        public string GeneratePassword()
        {
            if (_passwordGenerator == null)
            {
                _passwordGenerator = new PasswordGenerator(PasswordConfiguration);
            }

            var password = _passwordGenerator.GeneratePassword();
            return password;
        }

        /// <inheritdoc />
        public override async Task<bool> CheckPasswordAsync(TUser user, string password)
        {
            // we cannot proceed if the user passed in does not have an identity
            if (user.HasIdentity == false)
            {
                return false;
            }

            // use the default behavior
            return await base.CheckPasswordAsync(user, password);
        }

        /// <summary>
        /// This is a special method that will reset the password but will raise the Password Changed event instead of the reset event
        /// </summary>
        /// <param name="userId">The userId</param>
        /// <param name="token">The reset password token</param>
        /// <param name="newPassword">The new password to set it to</param>
        /// <returns>The <see cref="IdentityResult"/></returns>
        /// <remarks>
        /// We use this because in the back office the only way an admin can change another user's password without first knowing their password
        /// is to generate a token and reset it, however, when we do this we want to track a password change, not a password reset
        /// </remarks>
        public virtual async Task<IdentityResult> ChangePasswordWithResetAsync(string userId, string token, string newPassword)
        {
            TUser user = await FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("Could not find user");
            }

            IdentityResult result = await ResetPasswordAsync(user, token, newPassword);
            return result;
        }

        /// <inheritdoc/>
        public override async Task<IdentityResult> SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            IdentityResult result = await base.SetLockoutEndDateAsync(user, lockoutEnd);

            // The way we unlock is by setting the lockoutEnd date to the current datetime
            if (!result.Succeeded || lockoutEnd < DateTimeOffset.UtcNow)
            {
                // Resets the login attempt fails back to 0 when unlock is clicked
                await ResetAccessFailedCountAsync(user);
            }

            return result;
        }

        /// <inheritdoc/>
        public override async Task<IdentityResult> ResetAccessFailedCountAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var lockoutStore = (IUserLockoutStore<TUser>)Store;
            var accessFailedCount = await GetAccessFailedCountAsync(user);

            if (accessFailedCount == 0)
            {
                return IdentityResult.Success;
            }

            await lockoutStore.ResetAccessFailedCountAsync(user, CancellationToken.None);

            return await UpdateAsync(user);
        }

        /// <summary>
        /// Overrides the Microsoft ASP.NET user management method
        /// </summary>
        /// <inheritdoc/>
        public override async Task<IdentityResult> AccessFailedAsync(TUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var lockoutStore = Store as IUserLockoutStore<TUser>;
            if (lockoutStore == null)
            {
                throw new NotSupportedException("The current user store does not implement " + typeof(IUserLockoutStore<>));
            }

            var count = await lockoutStore.IncrementAccessFailedCountAsync(user, CancellationToken.None);

            if (count >= Options.Lockout.MaxFailedAccessAttempts)
            {
                await lockoutStore.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.Add(Options.Lockout.DefaultLockoutTimeSpan), CancellationToken.None);

                // NOTE: in normal aspnet identity this would do set the number of failed attempts back to 0
                // here we are persisting the value for the back office
            }

            IdentityResult result = await UpdateAsync(user);
            return result;
        }

    }
}
