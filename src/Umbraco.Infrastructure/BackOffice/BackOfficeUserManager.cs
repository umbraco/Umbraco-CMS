using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Security;
using Umbraco.Extensions;
using Umbraco.Net;

namespace Umbraco.Core.BackOffice
{
    public class BackOfficeUserManager : BackOfficeUserManager<BackOfficeIdentityUser>, IBackOfficeUserManager
    {
        public BackOfficeUserManager(
            IIpResolver ipResolver,
            IUserStore<BackOfficeIdentityUser> store,
            IOptions<BackOfficeIdentityOptions> optionsAccessor,
            IPasswordHasher<BackOfficeIdentityUser> passwordHasher,
            IEnumerable<IUserValidator<BackOfficeIdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<BackOfficeIdentityUser>> passwordValidators,
            BackOfficeLookupNormalizer keyNormalizer,
            BackOfficeIdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<BackOfficeIdentityUser>> logger,
            IOptions<UserPasswordConfigurationSettings> passwordConfiguration)
            : base(ipResolver, store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger, passwordConfiguration)
        {
        }
    }

    public class BackOfficeUserManager<T> : UserManager<T>
        where T : BackOfficeIdentityUser
    {
        private PasswordGenerator _passwordGenerator;

        public BackOfficeUserManager(
            IIpResolver ipResolver,
            IUserStore<T> store,
            IOptions<BackOfficeIdentityOptions> optionsAccessor,
            IPasswordHasher<T> passwordHasher,
            IEnumerable<IUserValidator<T>> userValidators,
            IEnumerable<IPasswordValidator<T>> passwordValidators,
            BackOfficeLookupNormalizer keyNormalizer,
            BackOfficeIdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<T>> logger,
            IOptions<UserPasswordConfigurationSettings> passwordConfiguration)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            IpResolver = ipResolver ?? throw new ArgumentNullException(nameof(ipResolver));
            PasswordConfiguration = passwordConfiguration.Value ?? throw new ArgumentNullException(nameof(passwordConfiguration));
        }

        #region What we do not currently support

        // We don't support an IUserClaimStore and don't need to (at least currently)
        public override bool SupportsUserClaim => false;

        // It would be nice to support this but we don't need to currently and that would require IQueryable support for our user service/repository
        public override bool SupportsQueryableUsers => false;

        /// <summary>
        /// Developers will need to override this to support custom 2 factor auth
        /// </summary>
        public override bool SupportsUserTwoFactor => false;

        // We haven't needed to support this yet, though might be necessary for 2FA
        public override bool SupportsUserPhoneNumber => false;

        #endregion

        /// <summary>
        /// Replace the underlying options property with our own strongly typed version
        /// </summary>
        public new BackOfficeIdentityOptions Options
        {
            get => (BackOfficeIdentityOptions)base.Options;
            set => base.Options = value;
        }

        /// <summary>
        /// Used to validate a user's session
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public virtual async Task<bool> ValidateSessionIdAsync(string userId, string sessionId)
        {
            var userSessionStore = Store as IUserSessionStore<T>;
            //if this is not set, for backwards compat (which would be super rare), we'll just approve it
            if (userSessionStore == null)  return true;

            return await userSessionStore.ValidateSessionIdAsync(userId, sessionId);
        }

        /// <summary>
        /// This will determine which password hasher to use based on what is defined in config
        /// </summary>
        /// <returns></returns>
        protected virtual IPasswordHasher<T> GetDefaultPasswordHasher(IPasswordConfiguration passwordConfiguration)
        {
            //we can use the user aware password hasher (which will be the default and preferred way)
            return new PasswordHasher<T>();
        }

        /// <summary>
        /// Gets/sets the default back office user password checker
        /// </summary>
        public IBackOfficeUserPasswordChecker BackOfficeUserPasswordChecker { get; set; }
        public IPasswordConfiguration PasswordConfiguration { get; protected set; }
        public IIpResolver IpResolver { get; }

        /// <summary>
        /// Helper method to generate a password for a user based on the current password validator
        /// </summary>
        /// <returns></returns>
        public string GeneratePassword()
        {
            if (_passwordGenerator == null) _passwordGenerator = new PasswordGenerator(PasswordConfiguration);
            var password = _passwordGenerator.GeneratePassword();
            return password;
        }

        /// <summary>
        /// Override to check the user approval value as well as the user lock out date, by default this only checks the user's locked out date
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <remarks>
        /// In the ASP.NET Identity world, there is only one value for being locked out, in Umbraco we have 2 so when checking this for Umbraco we need to check both values
        /// </remarks>
        public override async Task<bool> IsLockedOutAsync(T user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (user.IsApproved == false)  return true;

            return await base.IsLockedOutAsync(user);
        }

        #region Overrides for password logic

        /// <summary>
        /// Logic used to validate a username and password
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <remarks>
        /// By default this uses the standard ASP.Net Identity approach which is:
        /// * Get password store
        /// * Call VerifyPasswordAsync with the password store + user + password
        /// * Uses the PasswordHasher.VerifyHashedPassword to compare the stored password
        ///
        /// In some cases people want simple custom control over the username/password check, for simplicity
        /// sake, developers would like the users to simply validate against an LDAP directory but the user
        /// data remains stored inside of Umbraco.
        /// See: http://issues.umbraco.org/issue/U4-7032 for the use cases.
        ///
        /// We've allowed this check to be overridden with a simple callback so that developers don't actually
        /// have to implement/override this class.
        /// </remarks>
        public override async Task<bool> CheckPasswordAsync(T user, string password)
        {
            if (BackOfficeUserPasswordChecker != null)
            {
                var result = await BackOfficeUserPasswordChecker.CheckPasswordAsync(user, password);

                if (user.HasIdentity == false)
                {
                    return false;
                }

                //if the result indicates to not fallback to the default, then return true if the credentials are valid
                if (result != BackOfficeUserPasswordCheckerResult.FallbackToDefaultChecker)
                {
                    return result == BackOfficeUserPasswordCheckerResult.ValidCredentials;
                }
            }

            //we cannot proceed if the user passed in does not have an identity
            if (user.HasIdentity == false)
                return false;

            //use the default behavior
            return await base.CheckPasswordAsync(user, password);
        }

        /// <summary>
        /// This is a special method that will reset the password but will raise the Password Changed event instead of the reset event
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        /// <remarks>
        /// We use this because in the back office the only way an admin can change another user's password without first knowing their password
        /// is to generate a token and reset it, however, when we do this we want to track a password change, not a password reset
        /// </remarks>
        public async Task<IdentityResult> ChangePasswordWithResetAsync(int userId, string token, string newPassword)
        {
            var user = await base.FindByIdAsync(userId.ToString());
            if (user == null) throw new InvalidOperationException("Could not find user");

            var result = await base.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
                RaisePasswordChangedEvent(null, userId); // TODO: How can we get the current user? we have not HttpContext (netstandard), we can make our own IPrincipalAccessor?
            return result;
        }

        public override async Task<IdentityResult> ChangePasswordAsync(T user, string currentPassword, string newPassword)
        {
            var result = await base.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
                RaisePasswordChangedEvent(null, user.Id); // TODO: How can we get the current user? we have not HttpContext (netstandard), we can make our own IPrincipalAccessor?
            return result;
        }

        /// <summary>
        /// Override to determine how to hash the password
        /// </summary>
        /// <param name="user"></param>
        /// <param name="newPassword"></param>
        /// <param name="validatePassword"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method is called anytime the password needs to be hashed for storage (i.e. including when reset password is used)
        /// </remarks>
        protected override async Task<IdentityResult> UpdatePasswordHash(T user, string newPassword, bool validatePassword)
        {
            user.LastPasswordChangeDateUtc = DateTime.UtcNow;

            if (validatePassword)
            {
                var validate = await ValidatePasswordAsync(user, newPassword);
                if (!validate.Succeeded)
                {
                    return validate;
                }
            }

            var passwordStore = Store as IUserPasswordStore<T>;
            if (passwordStore == null) throw new NotSupportedException("The current user store does not implement " + typeof(IUserPasswordStore<>));

            var hash = newPassword != null ? PasswordHasher.HashPassword(user, newPassword) : null;
            await passwordStore.SetPasswordHashAsync(user, hash, CancellationToken);
            await UpdateSecurityStampInternal(user);
            return IdentityResult.Success;
        }

        /// <summary>
        /// This is copied from the underlying .NET base class since they decided to not expose it
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task UpdateSecurityStampInternal(T user)
        {
            if (SupportsUserSecurityStamp == false) return;
            await GetSecurityStore().SetSecurityStampAsync(user, NewSecurityStamp(), CancellationToken.None);
        }

        /// <summary>
        /// This is copied from the underlying .NET base class since they decided to not expose it
        /// </summary>
        /// <returns></returns>
        private IUserSecurityStampStore<T> GetSecurityStore()
        {
            var store = Store as IUserSecurityStampStore<T>;
            if (store == null) throw new NotSupportedException("The current user store does not implement " + typeof(IUserSecurityStampStore<>));
            return store;
        }

        /// <summary>
        /// This is copied from the underlying .NET base class since they decided to not expose it
        /// </summary>
        /// <returns></returns>
        private static string NewSecurityStamp()
        {
            return Guid.NewGuid().ToString();
        }

        #endregion

        public override async Task<IdentityResult> SetLockoutEndDateAsync(T user, DateTimeOffset? lockoutEnd)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var result = await base.SetLockoutEndDateAsync(user, lockoutEnd);

            // The way we unlock is by setting the lockoutEnd date to the current datetime
            if (result.Succeeded && lockoutEnd >= DateTimeOffset.UtcNow)
            {
                RaiseAccountLockedEvent(null, user.Id); // TODO: How can we get the current user? we have not HttpContext (netstandard), we can make our own IPrincipalAccessor?
            }
            else
            {
                RaiseAccountUnlockedEvent(null, user.Id); // TODO: How can we get the current user? we have not HttpContext (netstandard), we can make our own IPrincipalAccessor?
                //Resets the login attempt fails back to 0 when unlock is clicked
                await ResetAccessFailedCountAsync(user);
            }

            return result;
        }

        public override async Task<IdentityResult> ResetAccessFailedCountAsync(T user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var lockoutStore = (IUserLockoutStore<T>)Store;
            var accessFailedCount = await GetAccessFailedCountAsync(user);

            if (accessFailedCount == 0)
                return IdentityResult.Success;

            await lockoutStore.ResetAccessFailedCountAsync(user, CancellationToken.None);
            //raise the event now that it's reset
            RaiseResetAccessFailedCountEvent(null, user.Id); // TODO: How can we get the current user? we have not HttpContext (netstandard), we can make our own IPrincipalAccessor?
            return await UpdateAsync(user);
        }

        /// <summary>
        /// Overrides the Microsoft ASP.NET user management method
        /// </summary>
        /// <param name="user"></param>
        /// <returns>
        /// returns a Async Task<IdentityResult />
        /// </returns>
        /// <remarks>
        /// Doesn't set fail attempts back to 0
        /// </remarks>
        public override async Task<IdentityResult> AccessFailedAsync(T user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var lockoutStore = Store as IUserLockoutStore<T>;
            if (lockoutStore == null) throw new NotSupportedException("The current user store does not implement " + typeof(IUserLockoutStore<>));

            var count = await lockoutStore.IncrementAccessFailedCountAsync(user, CancellationToken.None);

            if (count >= Options.Lockout.MaxFailedAccessAttempts)
            {
                await lockoutStore.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.Add(Options.Lockout.DefaultLockoutTimeSpan),
                    CancellationToken.None);
                //NOTE: in normal aspnet identity this would do set the number of failed attempts back to 0
                //here we are persisting the value for the back office
            }

            var result = await UpdateAsync(user);

            //Slightly confusing: this will return a Success if we successfully update the AccessFailed count
            if (result.Succeeded)
            {
                // TODO: This may no longer be the case in netcore, we'll need to see about that
                RaiseLoginFailedEvent(null, user.Id); // TODO: How can we get the current user? we have not HttpContext (netstandard), we can make our own IPrincipalAccessor?
            }

            return result;
        }

        private IdentityAuditEventArgs CreateArgs(AuditEvent auditEvent, IPrincipal currentUser, int affectedUserId, string affectedUsername)
        {
            var umbIdentity = currentUser?.GetUmbracoIdentity();
            var currentUserId = umbIdentity?.GetUserId<int?>() ?? Constants.Security.SuperUserId;
            var ip = IpResolver.GetCurrentRequestIpAddress();
            return new IdentityAuditEventArgs(auditEvent, ip, currentUserId, string.Empty, affectedUserId, affectedUsername);
        }
        private IdentityAuditEventArgs CreateArgs(AuditEvent auditEvent, BackOfficeIdentityUser currentUser, int affectedUserId, string affectedUsername)
        {
            var currentUserId = currentUser.Id;
            var ip = IpResolver.GetCurrentRequestIpAddress();
            return new IdentityAuditEventArgs(auditEvent, ip, currentUserId, string.Empty, affectedUserId, affectedUsername);
        }

        // TODO: Review where these are raised and see if they can be simplified and either done in the this usermanager or the signin manager, lastly we'll resort to the authenticaiton controller
        // In some cases it will be nicer/easier to not pass in IPrincipal
        [UmbracoVolatile]
        public void RaiseAccountLockedEvent(IPrincipal currentUser, int userId) => OnAccountLocked(CreateArgs(AuditEvent.AccountLocked, currentUser, userId, string.Empty));

        [UmbracoVolatile]
        public void RaiseAccountUnlockedEvent(IPrincipal currentUser, int userId) => OnAccountUnlocked(CreateArgs(AuditEvent.AccountUnlocked, currentUser, userId, string.Empty));

        [UmbracoVolatile]
        public void RaiseForgotPasswordRequestedEvent(IPrincipal currentUser, int userId) => OnForgotPasswordRequested(CreateArgs(AuditEvent.ForgotPasswordRequested, currentUser, userId, string.Empty));

        [UmbracoVolatile]
        public void RaiseForgotPasswordChangedSuccessEvent(IPrincipal currentUser, int userId) => OnForgotPasswordChangedSuccess(CreateArgs(AuditEvent.ForgotPasswordChangedSuccess, currentUser, userId, string.Empty));

        [UmbracoVolatile]
        public void RaiseLoginFailedEvent(IPrincipal currentUser, int userId) => OnLoginFailed(CreateArgs(AuditEvent.LoginFailed, currentUser, userId, string.Empty));

        [UmbracoVolatile]
        public void RaiseInvalidLoginAttemptEvent(IPrincipal currentUser, string username) => OnLoginFailed(CreateArgs(AuditEvent.LoginFailed, currentUser, Constants.Security.SuperUserId, username));

        [UmbracoVolatile]
        public void RaiseLoginRequiresVerificationEvent(IPrincipal currentUser, int userId) => OnLoginRequiresVerification(CreateArgs(AuditEvent.LoginRequiresVerification, currentUser, userId, string.Empty));

        [UmbracoVolatile]
        public void RaiseLoginSuccessEvent(BackOfficeIdentityUser currentUser, int userId) => OnLoginSuccess(CreateArgs(AuditEvent.LoginSucces, currentUser, userId, string.Empty));

        [UmbracoVolatile]
        public void RaiseLogoutSuccessEvent(IPrincipal currentUser, int userId) => OnLogoutSuccess(CreateArgs(AuditEvent.LogoutSuccess, currentUser, userId, string.Empty));

        [UmbracoVolatile]
        public void RaisePasswordChangedEvent(IPrincipal currentUser, int userId) => OnPasswordChanged(CreateArgs(AuditEvent.LogoutSuccess, currentUser, userId, string.Empty));

        [UmbracoVolatile]
        public void RaiseResetAccessFailedCountEvent(IPrincipal currentUser, int userId) => OnResetAccessFailedCount(CreateArgs(AuditEvent.ResetAccessFailedCount, currentUser, userId, string.Empty));

        public static event EventHandler AccountLocked;
        public static event EventHandler AccountUnlocked;
        public static event EventHandler ForgotPasswordRequested;
        public static event EventHandler ForgotPasswordChangedSuccess;
        public static event EventHandler LoginFailed;
        public static event EventHandler LoginRequiresVerification;
        public static event EventHandler LoginSuccess;
        public static event EventHandler LogoutSuccess;
        public static event EventHandler PasswordChanged;
        public static event EventHandler PasswordReset;
        public static event EventHandler ResetAccessFailedCount;

        protected virtual void OnAccountLocked(IdentityAuditEventArgs e) => AccountLocked?.Invoke(this, e);

        protected virtual void OnAccountUnlocked(IdentityAuditEventArgs e) => AccountUnlocked?.Invoke(this, e);

        protected virtual void OnForgotPasswordRequested(IdentityAuditEventArgs e) => ForgotPasswordRequested?.Invoke(this, e);

        protected virtual void OnForgotPasswordChangedSuccess(IdentityAuditEventArgs e) => ForgotPasswordChangedSuccess?.Invoke(this, e);

        protected virtual void OnLoginFailed(IdentityAuditEventArgs e) => LoginFailed?.Invoke(this, e);

        protected virtual void OnLoginRequiresVerification(IdentityAuditEventArgs e) => LoginRequiresVerification?.Invoke(this, e);

        protected virtual void OnLoginSuccess(IdentityAuditEventArgs e) => LoginSuccess?.Invoke(this, e);

        protected virtual void OnLogoutSuccess(IdentityAuditEventArgs e) => LogoutSuccess?.Invoke(this, e);

        protected virtual void OnPasswordChanged(IdentityAuditEventArgs e) => PasswordChanged?.Invoke(this, e);

        protected virtual void OnPasswordReset(IdentityAuditEventArgs e) => PasswordReset?.Invoke(this, e);

        protected virtual void OnResetAccessFailedCount(IdentityAuditEventArgs e) => ResetAccessFailedCount?.Invoke(this, e);
    }
}
