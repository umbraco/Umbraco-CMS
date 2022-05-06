namespace Umbraco.Cms.Core.Security;

/// <summary>
///     This class is used by events raised from the BackofficeUserManager
/// </summary>
public class IdentityAuditEventArgs : EventArgs
{
    /// <summary>
    ///     Default constructor
    /// </summary>
    /// <param name="action"></param>
    /// <param name="ipAddress"></param>
    /// <param name="comment"></param>
    /// <param name="performingUser"></param>
    /// <param name="affectedUser"></param>
    public IdentityAuditEventArgs(AuditEvent action, string ipAddress, string performingUser, string comment,
        string affectedUser, string affectedUsername)
    {
        DateTimeUtc = DateTime.UtcNow;
        Action = action;
        IpAddress = ipAddress;
        Comment = comment;
        PerformingUser = performingUser;
        AffectedUsername = affectedUsername;
        AffectedUser = affectedUser;
    }

    public IdentityAuditEventArgs(AuditEvent action, string ipAddress, string performingUser, string comment,
        string affectedUsername)
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
