using System;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Compose;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.Common.Security
{
    /// <summary>
    /// Binds to events to write audit logs for the <see cref="IBackOfficeUserManager"/>
    /// </summary>
    internal class BackOfficeUserManagerAuditer : IDisposable
    {
        private readonly IAuditService _auditService;
        private readonly IUserService _userService;
        private readonly GlobalSettings _globalSettings;
        private bool _disposedValue;

        public BackOfficeUserManagerAuditer(IAuditService auditService, IUserService userService, IOptions<GlobalSettings> globalSettings)
        {
            _auditService = auditService;
            _userService = userService;
            _globalSettings = globalSettings.Value;
        }

        /// <summary>
        /// Binds to events to start auditing
        /// </summary>
        public void Start()
        {
            // NOTE: This was migrated as-is from v8 including these missing entries
            // TODO: See note about static events in BackOfficeUserManager
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

        private IUser GetPerformingUser(int userId)
        {
            var found = userId >= 0 ? _userService.GetUserById(userId) : null;
            return found ?? AuditEventsComponent.UnknownUser(_globalSettings);
        }

        private static string FormatEmail(IMembershipUser user)
        {
            return user == null ? string.Empty : user.Email.IsNullOrWhiteSpace() ? "" : $"<{user.Email}>";
        }

        private void OnLoginSuccess(object sender, IdentityAuditEventArgs args)
        {
            var performingUser = GetPerformingUser(args.PerformingUser);
            WriteAudit(performingUser, args.AffectedUser, args.IpAddress, "umbraco/user/sign-in/login", "login success");
        }

        private void OnLogoutSuccess(object sender, IdentityAuditEventArgs args)
        {
            var performingUser = GetPerformingUser(args.PerformingUser);
            WriteAudit(performingUser, args.AffectedUser, args.IpAddress, "umbraco/user/sign-in/logout", "logout success");
        }

        private void OnPasswordReset(object sender, IdentityAuditEventArgs args)
        {
            WriteAudit(args.PerformingUser, args.AffectedUser, args.IpAddress, "umbraco/user/password/reset", "password reset");
        }

        private void OnPasswordChanged(object sender, IdentityAuditEventArgs args)
        {
            WriteAudit(args.PerformingUser, args.AffectedUser, args.IpAddress, "umbraco/user/password/change", "password change");
        }

        private void OnLoginFailed(object sender, IdentityAuditEventArgs args)
        {
            WriteAudit(args.PerformingUser, 0, args.IpAddress, "umbraco/user/sign-in/failed", "login failed", affectedDetails: "");
        }

        private void OnForgotPasswordChange(object sender, IdentityAuditEventArgs args)
        {
            WriteAudit(args.PerformingUser, args.AffectedUser, args.IpAddress, "umbraco/user/password/forgot/change", "password forgot/change");
        }

        private void OnForgotPasswordRequest(object sender, IdentityAuditEventArgs args)
        {
            WriteAudit(args.PerformingUser, args.AffectedUser, args.IpAddress, "umbraco/user/password/forgot/request", "password forgot/request");
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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
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
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
