using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Extensions;
using Umbraco.Net;
using Umbraco.Web.Models.ContentEditing;


namespace Umbraco.Web.Common.Security
{
    public class BackOfficeUserManager : UmbracoUserManager<BackOfficeIdentityUser, UserPasswordConfigurationSettings>, IBackOfficeUserManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

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
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserManager<BackOfficeIdentityUser>> logger,
            IOptions<UserPasswordConfigurationSettings> passwordConfiguration)
            : base(ipResolver, store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger, passwordConfiguration)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets or sets the default back office user password checker
        /// </summary>
        public IBackOfficeUserPasswordChecker BackOfficeUserPasswordChecker { get; set; } // TODO: This isn't a good way to set this, it needs to be injected

        /// <inheritdoc />
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
        public override async Task<bool> CheckPasswordAsync(BackOfficeIdentityUser user, string password)
        {
            if (BackOfficeUserPasswordChecker != null)
            {
                BackOfficeUserPasswordCheckerResult result = await BackOfficeUserPasswordChecker.CheckPasswordAsync(user, password);

                if (user.HasIdentity == false)
                {
                    return false;
                }

                // if the result indicates to not fallback to the default, then return true if the credentials are valid
                if (result != BackOfficeUserPasswordCheckerResult.FallbackToDefaultChecker)
                {
                    return result == BackOfficeUserPasswordCheckerResult.ValidCredentials;
                }
            }

            // use the default behavior
            return await base.CheckPasswordAsync(user, password);
        }

        /// <summary>
        /// Override to check the user approval value as well as the user lock out date, by default this only checks the user's locked out date
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns>True if the user is locked out, else false</returns>
        /// <remarks>
        /// In the ASP.NET Identity world, there is only one value for being locked out, in Umbraco we have 2 so when checking this for Umbraco we need to check both values
        /// </remarks>
        public override async Task<bool> IsLockedOutAsync(BackOfficeIdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user.IsApproved == false)
            {
                return true;
            }

            return await base.IsLockedOutAsync(user);
        }

        public override async Task<IdentityResult> AccessFailedAsync(BackOfficeIdentityUser user)
        {
            IdentityResult result = await base.AccessFailedAsync(user);

            // Slightly confusing: this will return a Success if we successfully update the AccessFailed count
            if (result.Succeeded)
            {
                RaiseLoginFailedEvent(_httpContextAccessor.HttpContext?.User, user.Id);
            }

            return result;
        }

        public override async Task<IdentityResult> ChangePasswordWithResetAsync(string userId, string token, string newPassword)
        {
            IdentityResult result = await base.ChangePasswordWithResetAsync(userId, token, newPassword);
            if (result.Succeeded)
            {
                RaisePasswordChangedEvent(_httpContextAccessor.HttpContext?.User, userId);
            }

            return result;
        }

        public override async Task<IdentityResult> ChangePasswordAsync(BackOfficeIdentityUser user, string currentPassword, string newPassword)
        {
            IdentityResult result = await base.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                RaisePasswordChangedEvent(_httpContextAccessor.HttpContext?.User, user.Id);
            }

            return result;
        }

        /// <inheritdoc/>
        public override async Task<IdentityResult> SetLockoutEndDateAsync(BackOfficeIdentityUser user, DateTimeOffset? lockoutEnd)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            IdentityResult result = await base.SetLockoutEndDateAsync(user, lockoutEnd);

            // The way we unlock is by setting the lockoutEnd date to the current datetime
            if (result.Succeeded && lockoutEnd >= DateTimeOffset.UtcNow)
            {
                RaiseAccountLockedEvent(_httpContextAccessor.HttpContext?.User, user.Id);
            }
            else
            {
                RaiseAccountUnlockedEvent(_httpContextAccessor.HttpContext?.User, user.Id);

                // Resets the login attempt fails back to 0 when unlock is clicked
                await ResetAccessFailedCountAsync(user);
            }

            return result;
        }

        /// <inheritdoc/>
        public override async Task<IdentityResult> ResetAccessFailedCountAsync(BackOfficeIdentityUser user)
        {
            IdentityResult result = await base.ResetAccessFailedCountAsync(user);

            // raise the event now that it's reset
            RaiseResetAccessFailedCountEvent(_httpContextAccessor.HttpContext?.User, user.Id);

            return result;
        }

        private string GetCurrentUserId(IPrincipal currentUser)
        {
            ClaimsIdentity umbIdentity = currentUser?.GetUmbracoIdentity();
            var currentUserId = umbIdentity?.GetUserId<string>() ?? Core.Constants.Security.SuperUserIdAsString;
            return currentUserId;
        }

        private IdentityAuditEventArgs CreateArgs(AuditEvent auditEvent, IPrincipal currentUser, string affectedUserId, string affectedUsername)
        {
            var currentUserId = GetCurrentUserId(currentUser);
            var ip = IpResolver.GetCurrentRequestIpAddress();
            return new IdentityAuditEventArgs(auditEvent, ip, currentUserId, string.Empty, affectedUserId, affectedUsername);
        }

        private IdentityAuditEventArgs CreateArgs(AuditEvent auditEvent, BackOfficeIdentityUser currentUser, string affectedUserId, string affectedUsername)
        {
            var currentUserId = currentUser.Id;
            var ip = IpResolver.GetCurrentRequestIpAddress();
            return new IdentityAuditEventArgs(auditEvent, ip, currentUserId, string.Empty, affectedUserId, affectedUsername);
        }

        // TODO: Review where these are raised and see if they can be simplified and either done in the this usermanager or the signin manager,
        // lastly we'll resort to the authentication controller but we should try to remove all instances of that occuring
        public void RaiseAccountLockedEvent(IPrincipal currentUser, string userId) => OnAccountLocked(CreateArgs(AuditEvent.AccountLocked, currentUser, userId, string.Empty));

        public void RaiseAccountUnlockedEvent(IPrincipal currentUser, string userId) => OnAccountUnlocked(CreateArgs(AuditEvent.AccountUnlocked, currentUser, userId, string.Empty));

        public void RaiseForgotPasswordRequestedEvent(IPrincipal currentUser, string userId) => OnForgotPasswordRequested(CreateArgs(AuditEvent.ForgotPasswordRequested, currentUser, userId, string.Empty));

        public void RaiseForgotPasswordChangedSuccessEvent(IPrincipal currentUser, string userId) => OnForgotPasswordChangedSuccess(CreateArgs(AuditEvent.ForgotPasswordChangedSuccess, currentUser, userId, string.Empty));

        public void RaiseLoginFailedEvent(IPrincipal currentUser, string userId) => OnLoginFailed(CreateArgs(AuditEvent.LoginFailed, currentUser, userId, string.Empty));

        public void RaiseLoginRequiresVerificationEvent(IPrincipal currentUser, string userId) => OnLoginRequiresVerification(CreateArgs(AuditEvent.LoginRequiresVerification, currentUser, userId, string.Empty));

        public void RaiseLoginSuccessEvent(IPrincipal currentUser, string userId) => OnLoginSuccess(CreateArgs(AuditEvent.LoginSucces, currentUser, userId, string.Empty));

        public SignOutAuditEventArgs RaiseLogoutSuccessEvent(IPrincipal currentUser, string userId)
        {
            var currentUserId = GetCurrentUserId(currentUser);
            var args = new SignOutAuditEventArgs(AuditEvent.LogoutSuccess, IpResolver.GetCurrentRequestIpAddress(), performingUser: currentUserId, affectedUser: userId);
            OnLogoutSuccess(args);
            return args;
        }

        public void RaisePasswordChangedEvent(IPrincipal currentUser, string userId) => OnPasswordChanged(CreateArgs(AuditEvent.LogoutSuccess, currentUser, userId, string.Empty));

        public void RaiseResetAccessFailedCountEvent(IPrincipal currentUser, string userId) => OnResetAccessFailedCount(CreateArgs(AuditEvent.ResetAccessFailedCount, currentUser, userId, string.Empty));

        public UserInviteEventArgs RaiseSendingUserInvite(IPrincipal currentUser, UserInvite invite, IUser createdUser)
        {
            var currentUserId = GetCurrentUserId(currentUser);
            var ip = IpResolver.GetCurrentRequestIpAddress();
            var args = new UserInviteEventArgs(ip, currentUserId, invite, createdUser);
            OnSendingUserInvite(args);
            return args;
        }

        public bool HasSendingUserInviteEventHandler => SendingUserInvite != null;

        // TODO: These static events are problematic. Moving forward we don't want static events at all but we cannot
        // have non-static events here because the user manager is a Scoped instance not a singleton
        // so we'll have to deal with this a diff way i.e. refactoring how events are done entirely
        public static event EventHandler<IdentityAuditEventArgs> AccountLocked;
        public static event EventHandler<IdentityAuditEventArgs> AccountUnlocked;
        public static event EventHandler<IdentityAuditEventArgs> ForgotPasswordRequested;
        public static event EventHandler<IdentityAuditEventArgs> ForgotPasswordChangedSuccess;
        public static event EventHandler<IdentityAuditEventArgs> LoginFailed;
        public static event EventHandler<IdentityAuditEventArgs> LoginRequiresVerification;
        public static event EventHandler<IdentityAuditEventArgs> LoginSuccess;
        public static event EventHandler<SignOutAuditEventArgs> LogoutSuccess;
        public static event EventHandler<IdentityAuditEventArgs> PasswordChanged;
        public static event EventHandler<IdentityAuditEventArgs> PasswordReset;
        public static event EventHandler<IdentityAuditEventArgs> ResetAccessFailedCount;

        /// <summary>
        /// Raised when a user is invited
        /// </summary>
        public static event EventHandler<UserInviteEventArgs> SendingUserInvite; // this event really has nothing to do with the user manager but was the most convenient place to put it

        protected virtual void OnAccountLocked(IdentityAuditEventArgs e) => AccountLocked?.Invoke(this, e);

        protected virtual void OnSendingUserInvite(UserInviteEventArgs e) => SendingUserInvite?.Invoke(this, e);

        protected virtual void OnAccountUnlocked(IdentityAuditEventArgs e) => AccountUnlocked?.Invoke(this, e);

        protected virtual void OnForgotPasswordRequested(IdentityAuditEventArgs e) => ForgotPasswordRequested?.Invoke(this, e);

        protected virtual void OnForgotPasswordChangedSuccess(IdentityAuditEventArgs e) => ForgotPasswordChangedSuccess?.Invoke(this, e);

        protected virtual void OnLoginFailed(IdentityAuditEventArgs e) => LoginFailed?.Invoke(this, e);

        protected virtual void OnLoginRequiresVerification(IdentityAuditEventArgs e) => LoginRequiresVerification?.Invoke(this, e);

        protected virtual void OnLoginSuccess(IdentityAuditEventArgs e) => LoginSuccess?.Invoke(this, e);

        protected virtual void OnLogoutSuccess(SignOutAuditEventArgs e) => LogoutSuccess?.Invoke(this, e);

        protected virtual void OnPasswordChanged(IdentityAuditEventArgs e) => PasswordChanged?.Invoke(this, e);

        protected virtual void OnPasswordReset(IdentityAuditEventArgs e) => PasswordReset?.Invoke(this, e);

        protected virtual void OnResetAccessFailedCount(IdentityAuditEventArgs e) => ResetAccessFailedCount?.Invoke(this, e);
    }
}
