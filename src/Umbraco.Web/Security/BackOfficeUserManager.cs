using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Net;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// Default back office user manager
    /// </summary>
    public class BackOfficeUserManager : BackOfficeUserManager<BackOfficeIdentityUser>
    {
        public const string OwinMarkerKey = "Umbraco.Web.Security.Identity.BackOfficeUserManagerMarker";

        public BackOfficeUserManager(
            IUserStore<BackOfficeIdentityUser, int> store,
            IdentityFactoryOptions<BackOfficeUserManager> options,
            IContentSection contentSectionConfig,
            IPasswordConfiguration passwordConfiguration,
            IIpResolver ipResolver,
            IGlobalSettings globalSettings)
            : base(store, passwordConfiguration, ipResolver)
        {
            if (options == null) throw new ArgumentNullException("options");
            InitUserManager(this, passwordConfiguration, options.DataProtectionProvider, contentSectionConfig, globalSettings);
        }

        #region Static Create methods

        /// <summary>
        /// Creates a BackOfficeUserManager instance with all default options and the default BackOfficeUserManager
        /// </summary>
        /// <param name="options"></param>
        /// <param name="userService"></param>
        /// <param name="entityService"></param>
        /// <param name="externalLoginService"></param>
        /// <param name="passwordConfiguration"></param>
        /// <param name="contentSectionConfig"></param>
        /// <param name="globalSettings"></param>
        /// <returns></returns>
        public static BackOfficeUserManager Create(
            IdentityFactoryOptions<BackOfficeUserManager> options,
            IUserService userService,
            IEntityService entityService,
            IExternalLoginService externalLoginService,
            UmbracoMapper mapper,
            IContentSection contentSectionConfig,
            IGlobalSettings globalSettings,
            IPasswordConfiguration passwordConfiguration,
            IIpResolver ipResolver)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (userService == null) throw new ArgumentNullException("userService");
            if (externalLoginService == null) throw new ArgumentNullException("externalLoginService");

            var store = new BackOfficeUserStore(userService, entityService, externalLoginService, globalSettings, mapper);
            var manager = new BackOfficeUserManager(store, options, contentSectionConfig, passwordConfiguration, ipResolver, globalSettings);
            return manager;
        }

        /// <summary>
        /// Creates a BackOfficeUserManager instance with all default options and a custom BackOfficeUserManager instance
        /// </summary>
        /// <param name="options"></param>
        /// <param name="customUserStore"></param>
        /// <param name="passwordConfiguration"></param>
        /// <param name="contentSectionConfig"></param>
        /// <returns></returns>
        public static BackOfficeUserManager Create(
            IdentityFactoryOptions<BackOfficeUserManager> options,
            BackOfficeUserStore customUserStore,
            IContentSection contentSectionConfig,
            IPasswordConfiguration passwordConfiguration,
            IIpResolver ipResolver,
            IGlobalSettings globalSettings)
        {
            var manager = new BackOfficeUserManager(customUserStore, options, contentSectionConfig, passwordConfiguration, ipResolver, globalSettings);
            return manager;
        }
        #endregion


    }

    /// <summary>
    /// Generic Back office user manager
    /// </summary>
    public class BackOfficeUserManager<T> : UserManager<T, int>
        where T : BackOfficeIdentityUser
    {
        private PasswordGenerator _passwordGenerator;

        public BackOfficeUserManager(IUserStore<T, int> store,
            IPasswordConfiguration passwordConfiguration,
            IIpResolver ipResolver)
            : base(store)
        {
            PasswordConfiguration = passwordConfiguration;
            IpResolver = ipResolver;
        }

        #region What we support do not currently

        // TODO: We could support this - but a user claims will mostly just be what is in the auth cookie
        public override bool SupportsUserClaim
        {
            get { return false; }
        }

        // TODO: Support this
        public override bool SupportsQueryableUsers
        {
            get { return false; }
        }

        /// <summary>
        /// Developers will need to override this to support custom 2 factor auth
        /// </summary>
        public override bool SupportsUserTwoFactor
        {
            get { return false; }
        }

        // TODO: Support this
        public override bool SupportsUserPhoneNumber
        {
            get { return false; }
        }
        #endregion

        public virtual async Task<ClaimsIdentity> GenerateUserIdentityAsync(T user)
        {
            // NOTE the authenticationType must match the umbraco one
            // defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await CreateIdentityAsync(user, Core.Constants.Security.BackOfficeAuthenticationType);
            return userIdentity;
        }

        /// <summary>
        /// Initializes the user manager with the correct options
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="passwordConfig"></param>
        /// <param name="dataProtectionProvider"></param>
        /// <param name="contentSectionConfig"></param>
        /// <returns></returns>
        protected void InitUserManager(
            BackOfficeUserManager<T> manager,
            IPasswordConfiguration passwordConfig,
            IDataProtectionProvider dataProtectionProvider,
            IContentSection contentSectionConfig,
            IGlobalSettings globalSettings)
        {
            // Configure validation logic for usernames
            manager.UserValidator = new BackOfficeUserValidator<T>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new ConfiguredPasswordValidator(passwordConfig);

            //use a custom hasher based on our membership provider
            manager.PasswordHasher = GetDefaultPasswordHasher(passwordConfig);

            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<T, int>(dataProtectionProvider.Create("ASP.NET Identity"))
                {
                    TokenLifespan = TimeSpan.FromDays(3)
                };
            }

            manager.UserLockoutEnabledByDefault = true;
            manager.MaxFailedAccessAttemptsBeforeLockout = passwordConfig.MaxFailedAccessAttemptsBeforeLockout;
            //NOTE: This just needs to be in the future, we currently don't support a lockout timespan, it's either they are locked
            // or they are not locked, but this determines what is set on the account lockout date which corresponds to whether they are
            // locked out or not.
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromDays(30);

            //custom identity factory for creating the identity object for which we auth against in the back office
            manager.ClaimsIdentityFactory = new BackOfficeClaimsIdentityFactory<T>();

            manager.EmailService = new EmailService(
                contentSectionConfig.NotificationEmailAddress,
                new EmailSender(globalSettings));

            //NOTE: Not implementing these, if people need custom 2 factor auth, they'll need to implement their own UserStore to support it

            //// Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            //// You can write your own provider and plug in here.
            //manager.RegisterTwoFactorProvider("PhoneCode", new PhoneNumberTokenProvider<ApplicationUser>
            //{
            //    MessageFormat = "Your security code is: {0}"
            //});
            //manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<ApplicationUser>
            //{
            //    Subject = "Security Code",
            //    BodyFormat = "Your security code is: {0}"
            //});

            //manager.SmsService = new SmsService();
        }

        /// <summary>
        /// Used to validate a user's session
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public virtual async Task<bool> ValidateSessionIdAsync(int userId, string sessionId)
        {
            var userSessionStore = Store as IUserSessionStore<BackOfficeIdentityUser, int>;
            //if this is not set, for backwards compat (which would be super rare), we'll just approve it
            if (userSessionStore == null)
                return true;

            return await userSessionStore.ValidateSessionIdAsync(userId, sessionId);
        }

        /// <summary>
        /// This will determine which password hasher to use based on what is defined in config
        /// </summary>
        /// <returns></returns>
        protected virtual IPasswordHasher GetDefaultPasswordHasher(IPasswordConfiguration passwordConfiguration)
        {
            //we can use the user aware password hasher (which will be the default and preferred way)
            return new UserAwarePasswordHasher(new PasswordSecurity(passwordConfiguration));
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
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <remarks>
        /// In the ASP.NET Identity world, there is only one value for being locked out, in Umbraco we have 2 so when checking this for Umbraco we need to check both values
        /// </remarks>
        public override async Task<bool> IsLockedOutAsync(int userId)
        {
            var user = await FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("No user found by id " + userId);
            if (user.IsApproved == false)
                return true;

            return await base.IsLockedOutAsync(userId);
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

        public override Task<IdentityResult> ResetPasswordAsync(int userId, string token, string newPassword)
        {
            var result = base.ResetPasswordAsync(userId, token, newPassword);
            return result;
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
        public Task<IdentityResult> ChangePasswordWithResetAsync(int userId, string token, string newPassword)
        {
            var result = base.ResetPasswordAsync(userId, token, newPassword);
            if (result.Result.Succeeded)
                RaisePasswordChangedEvent(userId);
            return result;
        }

        public override Task<IdentityResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var result = base.ChangePasswordAsync(userId, currentPassword, newPassword);
            if (result.Result.Succeeded)
                RaisePasswordChangedEvent(userId);
            return result;
        }

        /// <summary>
        /// Override to determine how to hash the password
        /// </summary>
        /// <param name="store"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        protected override async Task<bool> VerifyPasswordAsync(IUserPasswordStore<T, int> store, T user, string password)
        {
            var userAwarePasswordHasher = PasswordHasher as IUserAwarePasswordHasher<BackOfficeIdentityUser, int>;
            if (userAwarePasswordHasher == null)
                return await base.VerifyPasswordAsync(store, user, password);

            var hash = await store.GetPasswordHashAsync(user);
            return userAwarePasswordHasher.VerifyHashedPassword(user, hash, password) != PasswordVerificationResult.Failed;
        }

        /// <summary>
        /// Override to determine how to hash the password
        /// </summary>
        /// <param name="passwordStore"></param>
        /// <param name="user"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method is called anytime the password needs to be hashed for storage (i.e. including when reset password is used)
        /// </remarks>
        protected override async Task<IdentityResult> UpdatePassword(IUserPasswordStore<T, int> passwordStore, T user, string newPassword)
        {
            user.LastPasswordChangeDateUtc = DateTime.UtcNow;
            var userAwarePasswordHasher = PasswordHasher as IUserAwarePasswordHasher<BackOfficeIdentityUser, int>;
            if (userAwarePasswordHasher == null)
                return await base.UpdatePassword(passwordStore, user, newPassword);

            var result = await PasswordValidator.ValidateAsync(newPassword);
            if (result.Succeeded == false)
                return result;

            await passwordStore.SetPasswordHashAsync(user, userAwarePasswordHasher.HashPassword(user, newPassword));
            await UpdateSecurityStampInternal(user);
            return IdentityResult.Success;


        }

        /// <summary>
        /// This is copied from the underlying .NET base class since they decided to not expose it
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task UpdateSecurityStampInternal(BackOfficeIdentityUser user)
        {
            if (SupportsUserSecurityStamp == false)
                return;
            await GetSecurityStore().SetSecurityStampAsync(user, NewSecurityStamp());
        }

        /// <summary>
        /// This is copied from the underlying .NET base class since they decided to not expose it
        /// </summary>
        /// <returns></returns>
        private IUserSecurityStampStore<BackOfficeIdentityUser, int> GetSecurityStore()
        {
            var store = Store as IUserSecurityStampStore<BackOfficeIdentityUser, int>;
            if (store == null)
                throw new NotSupportedException("The current user store does not implement " + typeof(IUserSecurityStampStore<>));
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

        public override async Task<IdentityResult> SetLockoutEndDateAsync(int userId, DateTimeOffset lockoutEnd)
        {
            var result = await base.SetLockoutEndDateAsync(userId, lockoutEnd);

            // The way we unlock is by setting the lockoutEnd date to the current datetime
            if (result.Succeeded && lockoutEnd >= DateTimeOffset.UtcNow)
            {
                RaiseAccountLockedEvent(userId);
            }
            else
            {
                RaiseAccountUnlockedEvent(userId);
                //Resets the login attempt fails back to 0 when unlock is clicked
                await ResetAccessFailedCountAsync(userId);

            }

            return result;
        }

        public override async Task<IdentityResult> ResetAccessFailedCountAsync(int userId)
        {
            var lockoutStore = (IUserLockoutStore<BackOfficeIdentityUser, int>)Store;
            var user = await FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("No user found by user id " + userId);

            var accessFailedCount = await GetAccessFailedCountAsync(user.Id);

            if (accessFailedCount == 0)
                return IdentityResult.Success;

            await lockoutStore.ResetAccessFailedCountAsync(user);
            //raise the event now that it's reset
            RaiseResetAccessFailedCountEvent(userId);
            return await UpdateAsync(user);
        }



        /// <summary>
        /// Overrides the Microsoft ASP.NET user management method
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>
        /// returns a Async Task<IdentityResult>
        /// </returns>
        /// <remarks>
        /// Doesn't set fail attempts back to 0
        /// </remarks>
        public override async Task<IdentityResult> AccessFailedAsync(int userId)
        {
            var lockoutStore = (IUserLockoutStore<BackOfficeIdentityUser, int>)Store;
            var user = await FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("No user found by user id " + userId);

            var count = await lockoutStore.IncrementAccessFailedCountAsync(user);

            if (count >= MaxFailedAccessAttemptsBeforeLockout)
            {
                await lockoutStore.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.Add(DefaultAccountLockoutTimeSpan));
                //NOTE: in normal aspnet identity this would do set the number of failed attempts back to 0
                //here we are persisting the value for the back office
            }

            var result = await UpdateAsync(user);

            //Slightly confusing: this will return a Success if we successfully update the AccessFailed count
            if (result.Succeeded)
                RaiseLoginFailedEvent(userId);

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

        internal void RaiseLogoutSuccessEvent(int userId)
        {
            OnLogoutSuccess(new IdentityAuditEventArgs(AuditEvent.LogoutSuccess, IpResolver.GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaisePasswordChangedEvent(int userId)
        {
            OnPasswordChanged(new IdentityAuditEventArgs(AuditEvent.PasswordChanged, IpResolver.GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseResetAccessFailedCountEvent(int userId)
        {
            OnResetAccessFailedCount(new IdentityAuditEventArgs(AuditEvent.ResetAccessFailedCount, IpResolver.GetCurrentRequestIpAddress(), affectedUser: userId));
        }

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

        protected virtual void OnAccountLocked(IdentityAuditEventArgs e)
        {
            if (AccountLocked != null) AccountLocked(this, e);
        }

        protected virtual void OnAccountUnlocked(IdentityAuditEventArgs e)
        {
            if (AccountUnlocked != null) AccountUnlocked(this, e);
        }

        protected virtual void OnForgotPasswordRequested(IdentityAuditEventArgs e)
        {
            if (ForgotPasswordRequested != null) ForgotPasswordRequested(this, e);
        }

        protected virtual void OnForgotPasswordChangedSuccess(IdentityAuditEventArgs e)
        {
            if (ForgotPasswordChangedSuccess != null) ForgotPasswordChangedSuccess(this, e);
        }

        protected virtual void OnLoginFailed(IdentityAuditEventArgs e)
        {
            if (LoginFailed != null) LoginFailed(this, e);
        }

        protected virtual void OnLoginRequiresVerification(IdentityAuditEventArgs e)
        {
            if (LoginRequiresVerification != null) LoginRequiresVerification(this, e);
        }

        protected virtual void OnLoginSuccess(IdentityAuditEventArgs e)
        {
            if (LoginSuccess != null) LoginSuccess(this, e);
        }

        protected virtual void OnLogoutSuccess(IdentityAuditEventArgs e)
        {
            if (LogoutSuccess != null) LogoutSuccess(this, e);
        }

        protected virtual void OnPasswordChanged(IdentityAuditEventArgs e)
        {
            if (PasswordChanged != null) PasswordChanged(this, e);
        }

        protected virtual void OnPasswordReset(IdentityAuditEventArgs e)
        {
            if (PasswordReset != null) PasswordReset(this, e);
        }

        protected virtual void OnResetAccessFailedCount(IdentityAuditEventArgs e)
        {
            if (ResetAccessFailedCount != null) ResetAccessFailedCount(this, e);
        }

    }

}
