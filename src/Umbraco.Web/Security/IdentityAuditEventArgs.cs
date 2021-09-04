using System;
using System.Threading;
using Umbraco.Core.Security;

namespace Umbraco.Web.Security
{

    /// <summary>
    /// This class is used by events raised from the BackofficeUserManager
    /// </summary>
    public class IdentityAuditEventArgs : EventArgs
    {
        /// <summary>
        /// The action that got triggered from the audit event
        /// </summary>
        public AuditEvent Action { get; private set; }

        /// <summary>
        /// Current date/time in UTC format
        /// </summary>
        public DateTime DateTimeUtc { get; private set; }

        /// <summary>
        /// The source IP address of the user performing the action
        /// </summary>
        public string IpAddress { get; private set; }

        /// <summary>
        /// The user affected by the event raised
        /// </summary>
        public int AffectedUser { get; private set; }

        /// <summary>
        /// If a user is performing an action on a different user, then this will be set. Otherwise it will be -1
        /// </summary>
        public int PerformingUser { get; private set; }

        /// <summary>
        /// An optional comment about the action being logged
        /// </summary>
        public string Comment { get; private set; }

        /// <summary>
        /// This property is always empty except in the LoginFailed event for an unknown user trying to login
        /// </summary>
        public string Username { get; private set; }


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="action"></param>
        /// <param name="ipAddress"></param>
        /// <param name="comment"></param>
        /// <param name="performingUser"></param>
        /// <param name="affectedUser"></param>
        public IdentityAuditEventArgs(AuditEvent action, string ipAddress, string comment = null, int performingUser = -1, int affectedUser = -1)
        {
            DateTimeUtc = DateTime.UtcNow;
            Action = action;

            IpAddress = ipAddress;
            Comment = comment;
            AffectedUser = affectedUser;

            PerformingUser = performingUser == -1
                ? GetCurrentRequestBackofficeUserId()
                : performingUser;
        }

        /// <summary>
        /// Creates an instance without a performing or affected user (the id will be set to -1)
        /// </summary>
        /// <param name="action"></param>
        /// <param name="ipAddress"></param>
        /// <param name="username"></param>
        /// <param name="comment"></param>
        public IdentityAuditEventArgs(AuditEvent action, string ipAddress, string username, string comment)
        {
            DateTimeUtc = DateTime.UtcNow;
            Action = action;

            IpAddress = ipAddress;
            Username = username;
            Comment = comment;

            PerformingUser = -1;
        }

        public IdentityAuditEventArgs(AuditEvent action, string ipAddress, string username, string comment, int performingUser)
        {
            DateTimeUtc = DateTime.UtcNow;
            Action = action;

            IpAddress = ipAddress;
            Username = username;
            Comment = comment;

            PerformingUser = performingUser == -1
                ? GetCurrentRequestBackofficeUserId()
                : performingUser;
        }

        /// <summary>
        /// Returns the current logged in backoffice user's Id logging if there is one
        /// </summary>
        /// <returns></returns>
        protected int GetCurrentRequestBackofficeUserId()
        {
            var userId = -1;
            var backOfficeIdentity = Thread.CurrentPrincipal.GetUmbracoIdentity();
            if (backOfficeIdentity != null)
                int.TryParse(backOfficeIdentity.Id.ToString(), out userId);
            return userId;
        }
    }

    public enum AuditEvent
    {
        AccountLocked,
        AccountUnlocked,
        ForgotPasswordRequested,
        ForgotPasswordChangedSuccess,
        LoginFailed,
        LoginRequiresVerification,
        LoginSucces,
        LogoutSuccess,
        PasswordChanged,
        PasswordReset,
        ResetAccessFailedCount,
        SendingUserInvite
    }
}
