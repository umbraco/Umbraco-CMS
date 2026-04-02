namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Specifies the type of audit event that occurred.
/// </summary>
public enum AuditEvent
{
    /// <summary>
    ///     The user account was locked due to too many failed login attempts.
    /// </summary>
    AccountLocked,

    /// <summary>
    ///     The user account was unlocked.
    /// </summary>
    AccountUnlocked,

    /// <summary>
    ///     A forgot password request was made.
    /// </summary>
    ForgotPasswordRequested,

    /// <summary>
    ///     The password was successfully changed via the forgot password flow.
    /// </summary>
    ForgotPasswordChangedSuccess,

    /// <summary>
    ///     A login attempt failed.
    /// </summary>
    LoginFailed,

    /// <summary>
    ///     The login requires additional verification (e.g., two-factor authentication).
    /// </summary>
    LoginRequiresVerification,

    /// <summary>
    ///     The login was successful.
    /// </summary>
    LoginSucces,

    /// <summary>
    ///     The user logged out successfully.
    /// </summary>
    LogoutSuccess,

    /// <summary>
    ///     The user's password was changed.
    /// </summary>
    PasswordChanged,

    /// <summary>
    ///     The user's password was reset.
    /// </summary>
    PasswordReset,

    /// <summary>
    ///     The failed access count was reset for the user.
    /// </summary>
    ResetAccessFailedCount,

    /// <summary>
    ///     A user invite is being sent.
    /// </summary>
    SendingUserInvite,
}

/// <summary>
///     This class is used by events raised from the BackofficeUserManager
/// </summary>
public class IdentityAuditEventArgs : EventArgs
{
    /// <summary>
    ///     Default constructor
    /// </summary>
    public IdentityAuditEventArgs(AuditEvent action, string ipAddress, string performingUser, string comment, string affectedUser, string affectedUsername)
    {
        DateTimeUtc = DateTime.UtcNow;
        Action = action;
        IpAddress = ipAddress;
        Comment = comment;
        PerformingUser = performingUser;
        AffectedUsername = affectedUsername;
        AffectedUser = affectedUser;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="IdentityAuditEventArgs" /> class
    ///     with the affected user defaulting to the super user.
    /// </summary>
    /// <param name="action">The audit action that was triggered.</param>
    /// <param name="ipAddress">The source IP address of the user performing the action.</param>
    /// <param name="performingUser">The identifier of the user performing the action.</param>
    /// <param name="comment">An optional comment about the action being logged.</param>
    /// <param name="affectedUsername">The username of the affected user.</param>
    public IdentityAuditEventArgs(AuditEvent action, string ipAddress, string performingUser, string comment, string affectedUsername)
        : this(action, ipAddress, performingUser, comment, Constants.Security.SuperUserIdAsString, affectedUsername)
    {
    }

    /// <summary>
    ///     The action that got triggered from the audit event
    /// </summary>
    public AuditEvent Action { get; }

    /// <summary>
    ///     Current date/time in UTC format
    /// </summary>
    public DateTime DateTimeUtc { get; }

    /// <summary>
    ///     The source IP address of the user performing the action
    /// </summary>
    public string IpAddress { get; }

    /// <summary>
    ///     The user affected by the event raised
    /// </summary>
    public string AffectedUser { get; }

    /// <summary>
    ///     If a user is performing an action on a different user, then this will be set. Otherwise it will be -1
    /// </summary>
    public string PerformingUser { get; }

    /// <summary>
    ///     An optional comment about the action being logged
    /// </summary>
    public string Comment { get; }

    /// <summary>
    ///     This property is always empty except in the LoginFailed event for an unknown user trying to login
    /// </summary>
    public string AffectedUsername { get; }
}
