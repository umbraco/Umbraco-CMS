using System;


namespace Umbraco.Core.Security
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
        public string AffectedUsername { get; private set; }


        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="action"></param>
        /// <param name="ipAddress"></param>
        /// <param name="comment"></param>
        /// <param name="performingUser"></param>
        /// <param name="affectedUser"></param>
        public IdentityAuditEventArgs(AuditEvent action, string ipAddress, int performingUser, string comment, int affectedUser, string affectedUsername)
        {
            DateTimeUtc = DateTime.UtcNow;
            Action = action;
            IpAddress = ipAddress;
            Comment = comment;
            PerformingUser = performingUser;
            AffectedUsername = affectedUsername;
            AffectedUser = affectedUser;
        }

        public IdentityAuditEventArgs(AuditEvent action, string ipAddress, int performingUser, string comment, string affectedUsername)
            : this(action, ipAddress, performingUser, comment, -1, affectedUsername)
        {
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
