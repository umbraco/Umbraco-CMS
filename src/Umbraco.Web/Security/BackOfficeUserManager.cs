using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
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

namespace Umbraco.Web.Security
{
    /// <summary>
    /// Default back office user manager
    /// </summary>
    public class BackOfficeUserManager : BackOfficeUserManager<BackOfficeIdentityUser>
    {
        public const string OwinMarkerKey = "Umbraco.Web.Security.Identity.BackOfficeUserManagerMarker";

        public BackOfficeUserManager(IUserStore<BackOfficeIdentityUser, int> store)
            : base(store)
        {
        }

        public BackOfficeUserManager(
            IUserStore<BackOfficeIdentityUser, int> store,
            IdentityFactoryOptions<BackOfficeUserManager> options,
            MembershipProviderBase membershipProvider,
            IContentSection contentSectionConfig)
            : base(store)
        {
            if (options == null) throw new ArgumentNullException("options");
            InitUserManager(this, membershipProvider, contentSectionConfig, options);
        }

        #region Static Create methods

        /// <summary>
        /// Creates a BackOfficeUserManager instance with all default options and the default BackOfficeUserManager
        /// </summary>
        /// <param name="options"></param>
        /// <param name="userService"></param>
        /// <param name="memberTypeService"></param>
        /// <param name="entityService"></param>
        /// <param name="externalLoginService"></param>
        /// <param name="membershipProvider"></param>
        /// <param name="contentSectionConfig"></param>
        /// <param name="globalSettings"></param>
        /// <returns></returns>
        public static BackOfficeUserManager Create(
            IdentityFactoryOptions<BackOfficeUserManager> options,
            IUserService userService,
            IMemberTypeService memberTypeService,
            IEntityService entityService,
            IExternalLoginService externalLoginService,
            MembershipProviderBase membershipProvider,
            UmbracoMapper mapper,
            IContentSection contentSectionConfig,
            IGlobalSettings globalSettings)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (userService == null) throw new ArgumentNullException("userService");
            if (memberTypeService == null) throw new ArgumentNullException("memberTypeService");
            if (externalLoginService == null) throw new ArgumentNullException("externalLoginService");

            var manager = new BackOfficeUserManager(
                new BackOfficeUserStore(userService, memberTypeService, entityService, externalLoginService, globalSettings, membershipProvider, mapper));
            manager.InitUserManager(manager, membershipProvider, contentSectionConfig, options);
            return manager;
        }

        /// <summary>
        /// Creates a BackOfficeUserManager instance with all default options and a custom BackOfficeUserManager instance
        /// </summary>
        /// <param name="options"></param>
        /// <param name="customUserStore"></param>
        /// <param name="membershipProvider"></param>
        /// <param name="contentSectionConfig"></param>
        /// <returns></returns>
        public static BackOfficeUserManager Create(
            IdentityFactoryOptions<BackOfficeUserManager> options,
            BackOfficeUserStore customUserStore,
            MembershipProviderBase membershipProvider,
            IContentSection contentSectionConfig)
        {
            var manager = new BackOfficeUserManager(customUserStore, options, membershipProvider, contentSectionConfig);
            return manager;
        }
        #endregion

        /// <summary>
        /// Initializes the user manager with the correct options
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="membershipProvider"></param>
        /// <param name="contentSectionConfig"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        protected void InitUserManager(
            BackOfficeUserManager manager,
            MembershipProviderBase membershipProvider,
            IContentSection contentSectionConfig,
            IdentityFactoryOptions<BackOfficeUserManager> options)
        {
            //NOTE: This method is mostly here for backwards compat
            base.InitUserManager(manager, membershipProvider, options.DataProtectionProvider, contentSectionConfig);
        }
    }

    /// <summary>
    /// Generic Back office user manager
    /// </summary>
    public class BackOfficeUserManager<T> : UserManager<T, int>
        where T : BackOfficeIdentityUser
    {
        public BackOfficeUserManager(IUserStore<T, int> store)
            : base(store)
        {
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
        /// <param name="membershipProvider">
        /// The <see cref="MembershipProviderBase"/> for the users called UsersMembershipProvider
        /// </param>
        /// <param name="dataProtectionProvider"></param>
        /// <param name="contentSectionConfig"></param>
        /// <returns></returns>
        protected void InitUserManager(
            BackOfficeUserManager<T> manager,
            MembershipProviderBase membershipProvider,
            IDataProtectionProvider dataProtectionProvider,
            IContentSection contentSectionConfig)
        {
            // Configure validation logic for usernames
            manager.UserValidator = new BackOfficeUserValidator<T>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new MembershipProviderPasswordValidator(membershipProvider);

            //use a custom hasher based on our membership provider
            manager.PasswordHasher = GetDefaultPasswordHasher(membershipProvider);

            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<T, int>(dataProtectionProvider.Create("ASP.NET Identity"))
                {
                    TokenLifespan = TimeSpan.FromDays(3)
                };
            }

            manager.UserLockoutEnabledByDefault = true;
            manager.MaxFailedAccessAttemptsBeforeLockout = membershipProvider.MaxInvalidPasswordAttempts;
            //NOTE: This just needs to be in the future, we currently don't support a lockout timespan, it's either they are locked
            // or they are not locked, but this determines what is set on the account lockout date which corresponds to whether they are
            // locked out or not.
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromDays(30);

            //custom identity factory for creating the identity object for which we auth against in the back office
            manager.ClaimsIdentityFactory = new BackOfficeClaimsIdentityFactory<T>();

            manager.EmailService = new EmailService(
                contentSectionConfig.NotificationEmailAddress,
                new EmailSender());

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
        protected virtual IPasswordHasher GetDefaultPasswordHasher(MembershipProviderBase provider)
        {
            //if the current user membership provider is unknown (this would be rare), then return the default password hasher
            if (provider.IsUmbracoUsersProvider() == false)
                return new PasswordHasher();

            //if the configured provider has legacy features enabled, then return the membership provider password hasher
            if (provider.AllowManuallyChangingPassword || provider.DefaultUseLegacyEncoding)
                return new MembershipProviderPasswordHasher(provider);

            //we can use the user aware password hasher (which will be the default and preferred way)
            return new UserAwareMembershipProviderPasswordHasher(provider);
        }

        /// <summary>
        /// Gets/sets the default back office user password checker
        /// </summary>
        public IBackOfficeUserPasswordChecker BackOfficeUserPasswordChecker { get; set; }

        /// <summary>
        /// Helper method to generate a password for a user based on the current password validator
        /// </summary>
        /// <returns></returns>
        public string GeneratePassword()
        {
            var passwordValidator = PasswordValidator as PasswordValidator;

            if (passwordValidator == null)
            {
                var membershipPasswordHasher = PasswordHasher as IMembershipProviderPasswordHasher;

                //get the real password validator, this should not be null but in some very rare cases it could be, in which case
                //we need to create a default password validator to use since we have no idea what it actually is or what it's rules are
                //this is an Edge Case!
                passwordValidator = PasswordValidator as PasswordValidator
                                    ?? (membershipPasswordHasher != null
                                        ? new MembershipProviderPasswordValidator(membershipPasswordHasher.MembershipProvider)
                                        : new PasswordValidator());
            }

            var password = Membership.GeneratePassword(
                passwordValidator.RequiredLength,
                passwordValidator.RequireNonLetterOrDigit ? 2 : 0);

            var random = new Random();

            var passwordChars = password.ToCharArray();

            if (passwordValidator.RequireDigit && passwordChars.ContainsAny(Enumerable.Range(48, 58).Select(x => (char)x)))
                password += Convert.ToChar(random.Next(48, 58));  // 0-9

            if (passwordValidator.RequireLowercase && passwordChars.ContainsAny(Enumerable.Range(97, 123).Select(x => (char)x)))
                password += Convert.ToChar(random.Next(97, 123));  // a-z

            if (passwordValidator.RequireUppercase && passwordChars.ContainsAny(Enumerable.Range(65, 91).Select(x => (char)x)))
                password += Convert.ToChar(random.Next(65, 91));  // A-Z

            if (passwordValidator.RequireNonLetterOrDigit && passwordChars.ContainsAny(Enumerable.Range(33, 48).Select(x => (char)x)))
                password += Convert.ToChar(random.Next(33, 48));  // symbols !"#$%&'()*+,-./

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
            if (result.Result.Succeeded)
                RaisePasswordResetEvent(userId);
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
            if (result.Succeeded && lockoutEnd > DateTimeOffset.UtcNow)
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
            OnAccountLocked(new IdentityAuditEventArgs(AuditEvent.AccountLocked, GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseAccountUnlockedEvent(int userId)
        {
            OnAccountUnlocked(new IdentityAuditEventArgs(AuditEvent.AccountUnlocked, GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseForgotPasswordRequestedEvent(int userId)
        {
            OnForgotPasswordRequested(new IdentityAuditEventArgs(AuditEvent.ForgotPasswordRequested, GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseForgotPasswordChangedSuccessEvent(int userId)
        {
            OnForgotPasswordChangedSuccess(new IdentityAuditEventArgs(AuditEvent.ForgotPasswordChangedSuccess, GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseLoginFailedEvent(int userId)
        {
            OnLoginFailed(new IdentityAuditEventArgs(AuditEvent.LoginFailed, GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseInvalidLoginAttemptEvent(string username)
        {
            OnLoginFailed(new IdentityAuditEventArgs(AuditEvent.LoginFailed, GetCurrentRequestIpAddress(), username, string.Format("Attempted login for username '{0}' failed", username)));
        }

        internal void RaiseLoginRequiresVerificationEvent(int userId)
        {
            OnLoginRequiresVerification(new IdentityAuditEventArgs(AuditEvent.LoginRequiresVerification, GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseLoginSuccessEvent(int userId)
        {
            OnLoginSuccess(new IdentityAuditEventArgs(AuditEvent.LoginSucces, GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal SignOutAuditEventArgs RaiseLogoutSuccessEvent(int userId)
        {
            var args = new SignOutAuditEventArgs(AuditEvent.LogoutSuccess, GetCurrentRequestIpAddress(), affectedUser: userId);
            OnLogoutSuccess(args);
            return args;
        }

        internal void RaisePasswordChangedEvent(int userId)
        {
            OnPasswordChanged(new IdentityAuditEventArgs(AuditEvent.PasswordChanged, GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        // TODO: I don't think this is required anymore since from 7.7 we no longer display the reset password checkbox since that didn't make sense.
        internal void RaisePasswordResetEvent(int userId)
        {
            OnPasswordReset(new IdentityAuditEventArgs(AuditEvent.PasswordReset, GetCurrentRequestIpAddress(), affectedUser: userId));
        }

        internal void RaiseResetAccessFailedCountEvent(int userId)
        {
            OnResetAccessFailedCount(new IdentityAuditEventArgs(AuditEvent.ResetAccessFailedCount, GetCurrentRequestIpAddress(), affectedUser: userId));
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

        protected virtual void OnPasswordReset(IdentityAuditEventArgs e) => PasswordReset?.Invoke(this, e);

        protected virtual void OnResetAccessFailedCount(IdentityAuditEventArgs e) => ResetAccessFailedCount?.Invoke(this, e);

        /// <summary>
        /// Returns the current request IP address for logging if there is one
        /// </summary>
        /// <returns></returns>
        protected virtual string GetCurrentRequestIpAddress()
        {
            // TODO: inject a service to get this value, we should not be relying on the old HttpContext.Current especially in the ASP.NET Identity world.
            var httpContext = HttpContext.Current == null ? (HttpContextBase)null : new HttpContextWrapper(HttpContext.Current);
            return httpContext.GetCurrentRequestIpAddress();
        }

    }

}
