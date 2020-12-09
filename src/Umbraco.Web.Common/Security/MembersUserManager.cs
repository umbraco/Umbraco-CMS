using System;
using System.Collections.Generic;
using System.Linq;
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
using Umbraco.Infrastructure.Security;
using Umbraco.Net;
using Umbraco.Web.Models.ContentEditing;


namespace Umbraco.Web.Common.Security
{
    public class MembersUserManager : UmbracoUserManager<MembersIdentityUser, MemberPasswordConfigurationSettings>, IMembersUserManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MembersUserManager(
            IIpResolver ipResolver,
            IUserStore<MembersIdentityUser> store,
            IOptions<MembersIdentityOptions> optionsAccessor,
            IPasswordHasher<MembersIdentityUser> passwordHasher,
            IEnumerable<IUserValidator<MembersIdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<MembersIdentityUser>> passwordValidators,
            //TODO: do we need members versions of this?
            BackOfficeLookupNormalizer keyNormalizer,
            BackOfficeIdentityErrorDescriber errors,
            IServiceProvider services,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserManager<MembersIdentityUser>> logger,
            IOptions<MemberPasswordConfigurationSettings> passwordConfiguration)
            : base(ipResolver, store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger, passwordConfiguration)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        private string GetCurrentUserId(IPrincipal currentUser)
        {
            UmbracoBackOfficeIdentity umbIdentity = currentUser?.GetUmbracoIdentity();
            var currentUserId = umbIdentity?.GetUserId<string>() ?? Core.Constants.Security.SuperUserIdAsString;
            return currentUserId;
        }

        private IdentityAuditEventArgs CreateArgs(AuditEvent auditEvent, IPrincipal currentUser, string affectedUserId, string affectedUsername)
        {
            var currentUserId = GetCurrentUserId(currentUser);
            var ip = IpResolver.GetCurrentRequestIpAddress();
            return new IdentityAuditEventArgs(auditEvent, ip, currentUserId, string.Empty, affectedUserId, affectedUsername);
        }


        // TODO: As per backoffice, review where these are raised and see if they can be simplified and either done in the this usermanager or the signin manager,
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

        public UserInviteEventArgs RaiseSendingUserInvite(IPrincipal currentUser, UserInvite invite, IUser createdUser)
        {
            var currentUserId = GetCurrentUserId(currentUser);
            var ip = IpResolver.GetCurrentRequestIpAddress();
            var args = new UserInviteEventArgs(ip, currentUserId, invite, createdUser);
            OnSendingUserInvite(args);
            return args;
        }

        public bool HasSendingUserInviteEventHandler => SendingUserInvite != null;

        // TODO: Comments re static events as per backofficeusermanager
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
