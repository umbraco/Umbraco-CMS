using System;
using System.Configuration.Provider;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using Umbraco.Core.Auditing;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Services;

namespace Umbraco.Core.Security
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
            MembershipProviderBase membershipProvider)
            : base(store)
        {
            if (options == null) throw new ArgumentNullException("options");
            InitUserManager(this, membershipProvider, options);
        }

        #region Static Create methods
        /// <summary>
        /// Creates a BackOfficeUserManager instance with all default options and the default BackOfficeUserManager 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="userService"></param>
        /// <param name="externalLoginService"></param>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        public static BackOfficeUserManager Create(
            IdentityFactoryOptions<BackOfficeUserManager> options,
            IUserService userService,
            IExternalLoginService externalLoginService,
            MembershipProviderBase membershipProvider)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (userService == null) throw new ArgumentNullException("userService");
            if (externalLoginService == null) throw new ArgumentNullException("externalLoginService");

            var manager = new BackOfficeUserManager(new BackOfficeUserStore(userService, externalLoginService, membershipProvider));
            manager.InitUserManager(manager, membershipProvider, options);
            return manager;
        }

        /// <summary>
        /// Creates a BackOfficeUserManager instance with all default options and a custom BackOfficeUserManager instance
        /// </summary>
        /// <param name="options"></param>
        /// <param name="customUserStore"></param>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        public static BackOfficeUserManager Create(
           IdentityFactoryOptions<BackOfficeUserManager> options,
           BackOfficeUserStore customUserStore,
           MembershipProviderBase membershipProvider)
        {
            var manager = new BackOfficeUserManager(customUserStore, options, membershipProvider);
            return manager;
        }
        #endregion

        /// <summary>
        /// Initializes the user manager with the correct options
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="membershipProvider"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        protected void InitUserManager(
            BackOfficeUserManager manager,
            MembershipProviderBase membershipProvider,
            IdentityFactoryOptions<BackOfficeUserManager> options)
        {
            //NOTE: This method is mostly here for backwards compat
            base.InitUserManager(manager, membershipProvider, options.DataProtectionProvider);
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

        //NOTE: Not sure if we really want/need to ever support this 
        public override bool SupportsUserClaim
        {
            get { return false; }
        }

        //TODO: Support this
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

        //TODO: Support this
        public override bool SupportsUserPhoneNumber
        {
            get { return false; }
        }
        #endregion

        /// <summary>
        /// Initializes the user manager with the correct options
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="membershipProvider"></param>
        /// <param name="dataProtectionProvider"></param>
        /// <returns></returns>
        protected void InitUserManager(
            BackOfficeUserManager<T> manager,
            MembershipProviderBase membershipProvider,
            IDataProtectionProvider dataProtectionProvider)
        {
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<T, int>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = membershipProvider.MinRequiredPasswordLength,
                RequireNonLetterOrDigit = membershipProvider.MinRequiredNonAlphanumericCharacters > 0,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false
                //TODO: Do we support the old regex match thing that membership providers used?
            };

            //use a custom hasher based on our membership provider
            manager.PasswordHasher = new MembershipPasswordHasher(membershipProvider);

            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<T, int>(dataProtectionProvider.Create("ASP.NET Identity"));
            }

            manager.UserLockoutEnabledByDefault = true;
            manager.MaxFailedAccessAttemptsBeforeLockout = membershipProvider.MaxInvalidPasswordAttempts;
            //NOTE: This just needs to be in the future, we currently don't support a lockout timespan, it's either they are locked
            // or they are not locked, but this determines what is set on the account lockout date which corresponds to whether they are
            // locked out or not.
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromDays(30);

            //custom identity factory for creating the identity object for which we auth against in the back office
            manager.ClaimsIdentityFactory = new BackOfficeClaimsIdentityFactory<T>();

            manager.EmailService = new EmailService();

            //NOTE: Not implementing these, if people need custom 2 factor auth, they'll need to implement their own UserStore to suport it

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
                //if the result indicates to not fallback to the default, then return true if the credentials are valid
                if (result != BackOfficeUserPasswordCheckerResult.FallbackToDefaultChecker)
                {
                    return result == BackOfficeUserPasswordCheckerResult.ValidCredentials;
                }
            }
            //use the default behavior
            return await base.CheckPasswordAsync(user, password);
        }

        /// <summary>
        /// Gets/sets the default back office user password checker
        /// </summary>
        public IBackOfficeUserPasswordChecker BackOfficeUserPasswordChecker { get; set; }

        public override Task<IdentityResult> SetLockoutEnabledAsync(int userId, bool enabled)
        {
            var result = base.SetLockoutEnabledAsync(userId, enabled);

            if (result.Result.Succeeded)
                OnAuthLocked(new IdentityAuditEventArgs(AuditEvent.AccountLocked)
                {
                    AffectedUser = userId
                });

            return result;
        }

        public override Task<IdentityResult> AccessFailedAsync(int userId)
        {
            var result = base.AccessFailedAsync(userId);

            if (result.Result.Succeeded)
                OnAuthAccessFailed(new IdentityAuditEventArgs(AuditEvent.AccessFailed)
                {
                    AffectedUser = userId
                });

            return result;
        }

        public override Task<IdentityResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var result = base.ChangePasswordAsync(userId, currentPassword, newPassword);

            if (result.Result.Succeeded)
                OnAuthPasswordChanged(new IdentityAuditEventArgs(AuditEvent.PasswordChanged)
                {
                    AffectedUser = userId
                });

            return result;
        }

        public override Task<IdentityResult> CreateAsync(T user)
        {
            var result = base.CreateAsync(user);

            if (result.Result.Succeeded)
                OnAuthAccountCreated(new IdentityAuditEventArgs(AuditEvent.AccountCreated)
                {
                    AffectedUser = user.Id
                });

            return result;
        }

        public override async Task<IdentityResult> ResetAccessFailedCountAsync(int userId)
        {
            var user = ApplicationContext.Current.Services.UserService.GetUserById(userId);

            if (user == null)
            {
                throw new ProviderException(string.Format("No user with the id {0} found", userId));
            }

            if (user.FailedPasswordAttempts > 0)
            {
                user.FailedPasswordAttempts = 0;
                ApplicationContext.Current.Services.UserService.Save(user);

                OnAuthResetAccessFailedCount(new IdentityAuditEventArgs(AuditEvent.ResetAccessFailedCount)
                {
                    AffectedUser = userId
                });
            }

            return await Task.FromResult(IdentityResult.Success);
        }

        internal void RaisePasswordChangedEvent(int userId)
        {
            OnAuthResetAccessFailedCount(new IdentityAuditEventArgs(AuditEvent.PasswordChanged)
            {
                AffectedUser = userId
            });
        }

        internal void RaisePasswordResetEvent(int userId)
        {
            OnAuthResetAccessFailedCount(new IdentityAuditEventArgs(AuditEvent.PasswordReset)
            {
                AffectedUser = userId
            });
        }

        internal void RaiseAccountLockedEvent(int userId)
        {
            OnAuthResetAccessFailedCount(new IdentityAuditEventArgs(AuditEvent.AccountLocked)
            {
                AffectedUser = userId
            });
        }

        internal void RaiseResetAccessFailedCountEvent(int userId)
        {
            OnAuthResetAccessFailedCount(new IdentityAuditEventArgs(AuditEvent.ResetAccessFailedCount)
            {
                AffectedUser = userId
            });
        }

        internal void RaiseLoginSuccessEvent(int userId)
        {
            OnAuthResetAccessFailedCount(new IdentityAuditEventArgs(AuditEvent.LoginSucces)
            {
                AffectedUser = userId
            });
        }

        internal void RaiseLogoutSuccessEvent(int userId)
        {
            OnAuthResetAccessFailedCount(new IdentityAuditEventArgs(AuditEvent.LogoutSuccess)
            {
                AffectedUser = userId
            });
        }

        public override Task<IdentityResult> UpdateAsync(T user)
        {
            var result = base.UpdateAsync(user);

            if (result.Result.Succeeded)
                OnAuthAccountUpdated(new IdentityAuditEventArgs(AuditEvent.AccountUpdated)
                {
                    AffectedUser = user.Id
                });

            return result;
        }

        /// <summary>
        /// Clears a lock so that the membership user can be validated.
        /// </summary>
        /// <param name="username">The membership user to clear the lock status for.</param>
        /// <returns>
        /// true if the membership user was successfully unlocked; otherwise, false.
        /// </returns>
        public bool UnlockUser(string username)
        {
            var user = ApplicationContext.Current.Services.UserService.GetByUsername(username);

            if (user == null)
            {
                throw new ProviderException(string.Format("No user with the username '{0}' found", username));
            }

            // Non need to update
            if (user.IsLockedOut == false) return true;

            user.IsLockedOut = false;
            user.FailedPasswordAttempts = 0;

            ApplicationContext.Current.Services.UserService.Save(user);

            OnAuthUnlocked(new IdentityAuditEventArgs(AuditEvent.AccountUnlocked)
            {
                AffectedUser = user.Id
            });

            return true;
        }

        public static event EventHandler AuthUnlocked;
        public static event EventHandler AuthLocked;
        public static event EventHandler AuthAccessFailed;
        public static event EventHandler AuthPasswordChanged;
        public static event EventHandler AuthAccountCreated;
        public static event EventHandler AuthResetAccessFailedCount;
        public static event EventHandler AuthAccountUpdated;

        protected virtual void OnAuthUnlocked(IdentityAuditEventArgs e)
        {
            if (AuthUnlocked != null) AuthUnlocked(this, e);
        }

        protected virtual void OnAuthLocked(IdentityAuditEventArgs e)
        {
            if (AuthLocked != null) AuthLocked(this, e);
        }

        protected virtual void OnAuthAccessFailed(IdentityAuditEventArgs e)
        {
            if (AuthAccessFailed != null) AuthAccessFailed(this, e);
        }

        protected virtual void OnAuthPasswordChanged(IdentityAuditEventArgs e)
        {
            if (AuthPasswordChanged != null) AuthPasswordChanged(this, e);
        }

        protected virtual void OnAuthAccountCreated(IdentityAuditEventArgs e)
        {
            if (AuthAccountCreated != null) AuthAccountCreated(this, e);
        }

        protected virtual void OnAuthResetAccessFailedCount(IdentityAuditEventArgs e)
        {
            if (AuthResetAccessFailedCount != null) AuthResetAccessFailedCount(this, e);
        }

        protected virtual void OnAuthAccountUpdated(IdentityAuditEventArgs e)
        {
            if (AuthAccountUpdated != null) AuthAccountUpdated(this, e);
        }
    }
}
