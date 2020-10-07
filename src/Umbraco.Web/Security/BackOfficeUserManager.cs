using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Owin.Security.DataProtection;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Mapping;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Net;
using Umbraco.Web.Models.Identity;

namespace Umbraco.Web.Security
{
    public class BackOfficeUserManager : BackOfficeUserManager<BackOfficeIdentityUser>
    {
        public const string OwinMarkerKey = "Umbraco.Web.Security.Identity.BackOfficeUserManagerMarker";

        public BackOfficeUserManager(
            IPasswordConfiguration passwordConfiguration,
            IIpResolver ipResolver,
            IUserStore<BackOfficeIdentityUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IEnumerable<IUserValidator<BackOfficeIdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<BackOfficeIdentityUser>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<UserManager<BackOfficeIdentityUser>> logger)
            : base(passwordConfiguration, ipResolver, store, optionsAccessor, userValidators, passwordValidators, keyNormalizer, errors, null, logger)
        {
            InitUserManager(this, dataProtectionProvider);
        }

        #region Static Create methods

        /// <summary>
        /// Creates a BackOfficeUserManager instance with all default options and the default BackOfficeUserManager
        /// </summary>
        public static BackOfficeUserManager Create(
            IUserService userService,
            IEntityService entityService,
            IExternalLoginService externalLoginService,
            IGlobalSettings globalSettings,
            UmbracoMapper mapper,
            IPasswordConfiguration passwordConfiguration,
            IIpResolver ipResolver,
            IdentityErrorDescriber errors,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<UserManager<BackOfficeIdentityUser>> logger)
        {
            var store = new BackOfficeUserStore(userService, entityService, externalLoginService, globalSettings, mapper);

            return Create(
                passwordConfiguration,
                ipResolver,
                store,
                errors,
                dataProtectionProvider,
                logger);
        }

        /// <summary>
        /// Creates a BackOfficeUserManager instance with all default options and a custom BackOfficeUserManager instance
        /// </summary>
        public static BackOfficeUserManager Create(
            IPasswordConfiguration passwordConfiguration,
            IIpResolver ipResolver,
            IUserStore<BackOfficeIdentityUser> customUserStore,
            IdentityErrorDescriber errors,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<UserManager<BackOfficeIdentityUser>> logger)
        {
            var options = new IdentityOptions();

            // Configure validation logic for usernames
            var userValidators = new List<UserValidator<BackOfficeIdentityUser>> { new BackOfficeUserValidator<BackOfficeIdentityUser>() };
            options.User.RequireUniqueEmail = true;

            // Configure validation logic for passwords
            var passwordValidators = new List<IPasswordValidator<BackOfficeIdentityUser>> { new PasswordValidator<BackOfficeIdentityUser>() };
            options.Password.RequiredLength = passwordConfiguration.RequiredLength;
            options.Password.RequireNonAlphanumeric = passwordConfiguration.RequireNonLetterOrDigit;
            options.Password.RequireDigit = passwordConfiguration.RequireDigit;
            options.Password.RequireLowercase = passwordConfiguration.RequireLowercase;
            options.Password.RequireUppercase = passwordConfiguration.RequireUppercase;

            // Ensure Umbraco security stamp claim type is used
            options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
            options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
            options.ClaimsIdentity.RoleClaimType = ClaimTypes.Role;
            options.ClaimsIdentity.SecurityStampClaimType = Constants.Web.SecurityStampClaimType;

            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = passwordConfiguration.MaxFailedAccessAttemptsBeforeLockout;
            //NOTE: This just needs to be in the future, we currently don't support a lockout timespan, it's either they are locked
            // or they are not locked, but this determines what is set on the account lockout date which corresponds to whether they are
            // locked out or not.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(30);

            return new BackOfficeUserManager(
                passwordConfiguration,
                ipResolver,
                customUserStore,
                new OptionsWrapper<IdentityOptions>(options),
                userValidators,
                passwordValidators,
                new NopLookupNormalizer(),
                errors,
                dataProtectionProvider,
                logger);
        }

        #endregion
    }

    public class BackOfficeUserManager<T> : UserManager<T>
        where T : BackOfficeIdentityUser
    {
        private PasswordGenerator _passwordGenerator;

        public BackOfficeUserManager(
            IPasswordConfiguration passwordConfiguration,
            IIpResolver ipResolver,
            IUserStore<T> store,
            IOptions<IdentityOptions> optionsAccessor,
            IEnumerable<IUserValidator<T>> userValidators,
            IEnumerable<IPasswordValidator<T>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<T>> logger)
            : base(store, optionsAccessor, null, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            PasswordConfiguration = passwordConfiguration ?? throw new ArgumentNullException(nameof(passwordConfiguration));
            IpResolver = ipResolver ?? throw new ArgumentNullException(nameof(ipResolver));
        }

        #region What we do not currently support
        // TODO: We could support this - but a user claims will mostly just be what is in the auth cookie
        public override bool SupportsUserClaim => false;

        // TODO: Support this
        public override bool SupportsQueryableUsers => false;

        /// <summary>
        /// Developers will need to override this to support custom 2 factor auth
        /// </summary>
        public override bool SupportsUserTwoFactor => false;

        // TODO: Support this
        public override bool SupportsUserPhoneNumber => false;
        #endregion

        /// <summary>
        /// Initializes the user manager with the correct options
        /// </summary>
        protected void InitUserManager(
            BackOfficeUserManager<T> manager,
            IDataProtectionProvider dataProtectionProvider)
        {
            // use a custom hasher based on our membership provider
            PasswordHasher = GetDefaultPasswordHasher(PasswordConfiguration);

            // set OWIN data protection token provider as default
            if (dataProtectionProvider != null)
            {
                manager.RegisterTokenProvider(
                    TokenOptions.DefaultProvider,
                    new OwinDataProtectorTokenProvider<T>(dataProtectionProvider.Create("ASP.NET Identity"))
                    {
                        TokenLifespan = TimeSpan.FromDays(3)
                    });
            }

            // register ASP.NET Core Identity token providers
            manager.RegisterTokenProvider(TokenOptions.DefaultEmailProvider, new EmailTokenProvider<T>());
            manager.RegisterTokenProvider(TokenOptions.DefaultPhoneProvider, new PhoneNumberTokenProvider<T>());
            manager.RegisterTokenProvider(TokenOptions.DefaultAuthenticatorProvider, new AuthenticatorTokenProvider<T>());
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
            return new UserAwarePasswordHasher<T>(new PasswordSecurity(passwordConfiguration));
        }

        /// <summary>
        /// Gets/sets the default back office user password checker
        /// </summary>
        public IBackOfficeUserPasswordChecker BackOfficeUserPasswordChecker { get; set; }
        public IPasswordConfiguration PasswordConfiguration { get; }
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
            if (result.Succeeded) RaisePasswordChangedEvent(userId);
            return result;
        }

        public override async Task<IdentityResult> ChangePasswordAsync(T user, string currentPassword, string newPassword)
        {
            var result = await base.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded) RaisePasswordChangedEvent(user.Id);
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
                RaiseAccountLockedEvent(user.Id);
            }
            else
            {
                RaiseAccountUnlockedEvent(user.Id);
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
            RaiseResetAccessFailedCountEvent(user.Id);
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
            if (result.Succeeded) RaiseLoginFailedEvent(user.Id);

            return result;
        }

        internal void RaiseAccountLockedEvent(int userId)
        {
            OnAccountLocked(new IdentityAuditEventArgs(AuditEvent.AccountLocked, IpResolver.GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseAccountUnlockedEvent(int userId)
        {
            OnAccountUnlocked(new IdentityAuditEventArgs(AuditEvent.AccountUnlocked, IpResolver.GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseForgotPasswordRequestedEvent(int userId)
        {
            OnForgotPasswordRequested(new IdentityAuditEventArgs(AuditEvent.ForgotPasswordRequested, IpResolver.GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseForgotPasswordChangedSuccessEvent(int userId)
        {
            OnForgotPasswordChangedSuccess(new IdentityAuditEventArgs(AuditEvent.ForgotPasswordChangedSuccess, IpResolver.GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseLoginFailedEvent(int userId)
        {
            OnLoginFailed(new IdentityAuditEventArgs(AuditEvent.LoginFailed, IpResolver.GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseInvalidLoginAttemptEvent(string username)
        {
            OnLoginFailed(new IdentityAuditEventArgs(AuditEvent.LoginFailed, IpResolver.GetCurrentRequestIpAddress(), username, string.Format("Attempted login for username '{0}' failed", username)));
        }

        internal void RaiseLoginRequiresVerificationEvent(int userId)
        {
            OnLoginRequiresVerification(new IdentityAuditEventArgs(AuditEvent.LoginRequiresVerification, IpResolver.GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseLoginSuccessEvent(int userId)
        {
            OnLoginSuccess(new IdentityAuditEventArgs(AuditEvent.LoginSucces, IpResolver.GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal SignOutAuditEventArgs RaiseLogoutSuccessEvent(int userId)
        {
            var args = new SignOutAuditEventArgs(AuditEvent.LogoutSuccess, IpResolver.GetCurrentRequestIpAddress(), affectedUser: userId);
            OnLogoutSuccess(args);
            return args;
        }

        internal void RaisePasswordChangedEvent(int userId)
        {
            OnPasswordChanged(new IdentityAuditEventArgs(AuditEvent.PasswordChanged, IpResolver.GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseResetAccessFailedCountEvent(int userId)
        {
            OnResetAccessFailedCount(new IdentityAuditEventArgs(AuditEvent.ResetAccessFailedCount, IpResolver.GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseSendingUserInvite(UserInviteEventArgs args) => OnSendingUserInvite(args);
        internal bool HasSendingUserInviteEventHandler => SendingUserInvite != null;

        // TODO: Not sure why these are not strongly typed events?? They should be in netcore!
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

        /// <summary>
        /// Raised when a user is invited
        /// </summary>
        public static event EventHandler SendingUserInvite; // this event really has nothing to do with the user manager but was the most convenient place to put it

        protected virtual void OnSendingUserInvite(UserInviteEventArgs e) => SendingUserInvite?.Invoke(this, e);

        protected virtual void OnAccountLocked(IdentityAuditEventArgs e) => AccountLocked?.Invoke(this, e);

        protected virtual void OnAccountUnlocked(IdentityAuditEventArgs e) => AccountUnlocked?.Invoke(this, e);

        protected virtual void OnForgotPasswordRequested(IdentityAuditEventArgs e) => ForgotPasswordRequested?.Invoke(this, e);

        protected virtual void OnForgotPasswordChangedSuccess(IdentityAuditEventArgs e) => ForgotPasswordChangedSuccess?.Invoke(this, e);

        protected virtual void OnLoginFailed(IdentityAuditEventArgs e) => LoginFailed?.Invoke(this, e);

        protected virtual void OnLoginRequiresVerification(IdentityAuditEventArgs e) => LoginRequiresVerification?.Invoke(this, e);

        protected virtual void OnLoginSuccess(IdentityAuditEventArgs e) => LoginSuccess?.Invoke(this, e);

        protected virtual void OnLogoutSuccess(IdentityAuditEventArgs e) => LogoutSuccess?.Invoke(this, e);

        protected virtual void OnPasswordChanged(IdentityAuditEventArgs e) => PasswordChanged?.Invoke(this, e);

        protected virtual void OnResetAccessFailedCount(IdentityAuditEventArgs e)
        {
            if (ResetAccessFailedCount != null) ResetAccessFailedCount(this, e);
        }
    }
}
