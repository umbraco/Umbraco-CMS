using System;
using Umbraco.Core;
using Umbraco.Core.Compose;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Security;

namespace Umbraco.Web.Compose
{
    public sealed class BackOfficeUserAuditEventsComponent : IComponent
    {
        private readonly IAuditService _auditService;
        private readonly IUserService _userService;

        public BackOfficeUserAuditEventsComponent(IAuditService auditService, IUserService userService)
        {
            _auditService = auditService;
            _userService = userService;
        }

        public void Initialize()
        {
            //BackOfficeUserManager.AccountLocked += ;
            //BackOfficeUserManager.AccountUnlocked += ;
            BackOfficeUserManager.ForgotPasswordRequested += OnForgotPasswordRequest;
            BackOfficeUserManager.ForgotPasswordChangedSuccess += OnForgotPasswordChange;
            BackOfficeUserManager.LoginFailed += OnLoginFailed;
            //BackOfficeUserManager.LoginRequiresVerification += ;
            BackOfficeUserManager.LoginSuccess += OnLoginSuccess;
            BackOfficeUserManager.LogoutSuccess += OnLogoutSuccess;
            BackOfficeUserManager.PasswordChanged += OnPasswordChanged;
            BackOfficeUserManager.PasswordReset += OnPasswordReset;
            //BackOfficeUserManager.ResetAccessFailedCount += ;
        }

        public void Terminate()
        {
            //BackOfficeUserManager.AccountLocked -= ;
            //BackOfficeUserManager.AccountUnlocked -= ;
            BackOfficeUserManager.ForgotPasswordRequested -= OnForgotPasswordRequest;
            BackOfficeUserManager.ForgotPasswordChangedSuccess -= OnForgotPasswordChange;
            BackOfficeUserManager.LoginFailed -= OnLoginFailed;
            //BackOfficeUserManager.LoginRequiresVerification -= ;
            BackOfficeUserManager.LoginSuccess -= OnLoginSuccess;
            BackOfficeUserManager.LogoutSuccess -= OnLogoutSuccess;
            BackOfficeUserManager.PasswordChanged -= OnPasswordChanged;
            BackOfficeUserManager.PasswordReset -= OnPasswordReset;
            //BackOfficeUserManager.ResetAccessFailedCount -= ;
        }

        private IUser GetPerformingUser(int userId)
        {
            var found = userId >= 0 ? _userService.GetUserById(userId) : null;
            return found ?? AuditEventsComponent.UnknownUser;
        }

        private static string FormatEmail(IMembershipUser user)
        {
            return user == null ? string.Empty : user.Email.IsNullOrWhiteSpace() ? "" : $"<{user.Email}>";
        }

        private void OnLoginSuccess(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs)
            {
                var performingUser = GetPerformingUser(identityArgs.PerformingUser);
                WriteAudit(performingUser, identityArgs.AffectedUser, identityArgs.IpAddress, "umbraco/user/sign-in/login", "login success");
            }
        }

        private void OnLogoutSuccess(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs)
            {
                var performingUser = GetPerformingUser(identityArgs.PerformingUser);
                WriteAudit(performingUser, identityArgs.AffectedUser, identityArgs.IpAddress, "umbraco/user/sign-in/logout", "logout success");
            }
        }

        private void OnPasswordReset(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs && identityArgs.PerformingUser >= 0)
            {
                WriteAudit(identityArgs.PerformingUser, identityArgs.AffectedUser, identityArgs.IpAddress, "umbraco/user/password/reset", "password reset");
            }
        }

        private void OnPasswordChanged(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs && identityArgs.PerformingUser >= 0)
            {
                WriteAudit(identityArgs.PerformingUser, identityArgs.AffectedUser, identityArgs.IpAddress, "umbraco/user/password/change", "password change");
            }
        }

        private void OnLoginFailed(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs && identityArgs.PerformingUser >= 0)
            {
                WriteAudit(identityArgs.PerformingUser, 0, identityArgs.IpAddress, "umbraco/user/sign-in/failed", "login failed", affectedDetails: "");
            }
        }

        private void OnForgotPasswordChange(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs && identityArgs.PerformingUser >= 0)
            {
                WriteAudit(identityArgs.PerformingUser, identityArgs.AffectedUser, identityArgs.IpAddress, "umbraco/user/password/forgot/change", "password forgot/change");
            }
        }

        private void OnForgotPasswordRequest(object sender, EventArgs args)
        {
            if (args is IdentityAuditEventArgs identityArgs && identityArgs.PerformingUser >= 0)
            {
                WriteAudit(identityArgs.PerformingUser, identityArgs.AffectedUser, identityArgs.IpAddress, "umbraco/user/password/forgot/request", "password forgot/request");
            }
        }

        private void WriteAudit(int performingId, int affectedId, string ipAddress, string eventType, string eventDetails, string affectedDetails = null)
        {
            var performingUser = _userService.GetUserById(performingId);

            var performingDetails = performingUser == null
                ? $"User UNKNOWN:{performingId}"
                : $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}";

            WriteAudit(performingId, performingDetails, affectedId, ipAddress, eventType, eventDetails, affectedDetails);
        }

        private void WriteAudit(IUser performingUser, int affectedId, string ipAddress, string eventType, string eventDetails)
        {
            var performingDetails = performingUser == null
                ? $"User UNKNOWN"
                : $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}";

            WriteAudit(performingUser?.Id ?? 0, performingDetails, affectedId, ipAddress, eventType, eventDetails);
        }

        private void WriteAudit(int performingId, string performingDetails, int affectedId, string ipAddress, string eventType, string eventDetails, string affectedDetails = null)
        {
            if (affectedDetails == null)
            {
                var affectedUser = _userService.GetUserById(affectedId);
                affectedDetails = affectedUser == null
                    ? $"User UNKNOWN:{affectedId}"
                    : $"User \"{affectedUser.Name}\" {FormatEmail(affectedUser)}";
            }

            _auditService.Write(performingId, performingDetails,
                ipAddress,
                DateTime.UtcNow,
                affectedId, affectedDetails,
                eventType, eventDetails);
        }
    }
}
