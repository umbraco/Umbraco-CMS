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
            BackOfficeUserManager.ForgotPasswordRequested += OnForgotPasswordRequest;
            BackOfficeUserManager.ForgotPasswordChangedSuccess += OnForgotPasswordChange;
            BackOfficeUserManager.LoginFailed += OnLoginFailed;
            BackOfficeUserManager.LoginSuccess += OnLoginSuccess;
            BackOfficeUserManager.LogoutSuccess += OnLogoutSuccess;
            BackOfficeUserManager.PasswordChanged += OnPasswordChanged;
            BackOfficeUserManager.PasswordReset += OnPasswordReset;
        }

        private IUser GetPerformingUser(string userId)
        {
            if (!int.TryParse(userId, out int asInt))
            {
                return AuditEventsComponent.UnknownUser(_globalSettings);
            }

            IUser found = asInt >= 0 ? _userService.GetUserById(asInt) : null;
            return found ?? AuditEventsComponent.UnknownUser(_globalSettings);
        }

        private static string FormatEmail(IMembershipUser user) => user == null ? string.Empty : user.Email.IsNullOrWhiteSpace() ? "" : $"<{user.Email}>";

        private void OnLoginSuccess(object sender, IdentityAuditEventArgs args)
        {
            var performingUser = GetPerformingUser(args.PerformingUser);
            WriteAudit(performingUser, args.AffectedUser, args.IpAddress, "umbraco/user/sign-in/login", "login success");
        }

        private void OnLogoutSuccess(object sender, IdentityAuditEventArgs args)
        {
            IUser performingUser = GetPerformingUser(args.PerformingUser);
            WriteAudit(performingUser, args.AffectedUser, args.IpAddress, "umbraco/user/sign-in/logout", "logout success");
        }

        private void OnPasswordReset(object sender, IdentityAuditEventArgs args) => WriteAudit(args.PerformingUser, args.AffectedUser, args.IpAddress, "umbraco/user/password/reset", "password reset");

        private void OnPasswordChanged(object sender, IdentityAuditEventArgs args) => WriteAudit(args.PerformingUser, args.AffectedUser, args.IpAddress, "umbraco/user/password/change", "password change");

        private void OnLoginFailed(object sender, IdentityAuditEventArgs args) => WriteAudit(args.PerformingUser, "0", args.IpAddress, "umbraco/user/sign-in/failed", "login failed", affectedDetails: "");

        private void OnForgotPasswordChange(object sender, IdentityAuditEventArgs args) => WriteAudit(args.PerformingUser, args.AffectedUser, args.IpAddress, "umbraco/user/password/forgot/change", "password forgot/change");

        private void OnForgotPasswordRequest(object sender, IdentityAuditEventArgs args) => WriteAudit(args.PerformingUser, args.AffectedUser, args.IpAddress, "umbraco/user/password/forgot/request", "password forgot/request");

        private void WriteAudit(string performingId, string affectedId, string ipAddress, string eventType, string eventDetails, string affectedDetails = null)
        {
            IUser performingUser = null;
            if (int.TryParse(performingId, out int asInt))
            {
                performingUser = _userService.GetUserById(asInt);
            }

            var performingDetails = performingUser == null
                ? $"User UNKNOWN:{performingId}"
                : $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}";

            if (!int.TryParse(performingId, out int performingIdAsInt))
            {
                performingIdAsInt = 0;
            }

            if (!int.TryParse(affectedId, out int affectedIdAsInt))
            {
                affectedIdAsInt = 0;
            }

            WriteAudit(performingIdAsInt, performingDetails, affectedIdAsInt, ipAddress, eventType, eventDetails, affectedDetails);
        }

        private void WriteAudit(IUser performingUser, string affectedId, string ipAddress, string eventType, string eventDetails)
        {
            var performingDetails = performingUser == null
                ? $"User UNKNOWN"
                : $"User \"{performingUser.Name}\" {FormatEmail(performingUser)}";

            if (!int.TryParse(affectedId, out int affectedIdInt))
            {
                affectedIdInt = 0;
            }

            WriteAudit(performingUser?.Id ?? 0, performingDetails, affectedIdInt, ipAddress, eventType, eventDetails);
        }

        private void WriteAudit(int performingId, string performingDetails, int affectedId, string ipAddress, string eventType, string eventDetails, string affectedDetails = null)
        {
            if (affectedDetails == null)
            {
                IUser affectedUser = _userService.GetUserById(affectedId);
                affectedDetails = affectedUser == null
                    ? $"User UNKNOWN:{affectedId}"
                    : $"User \"{affectedUser.Name}\" {FormatEmail(affectedUser)}";
            }

            _auditService.Write(
                performingId,
                performingDetails,
                ipAddress,
                DateTime.UtcNow,
                affectedId,
                affectedDetails,
                eventType,
                eventDetails);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    BackOfficeUserManager.ForgotPasswordRequested -= OnForgotPasswordRequest;
                    BackOfficeUserManager.ForgotPasswordChangedSuccess -= OnForgotPasswordChange;
                    BackOfficeUserManager.LoginFailed -= OnLoginFailed;
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
