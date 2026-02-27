namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Represents the result of a public access check for protected content.
/// </summary>
public enum PublicAccessStatus
{
    /// <summary>
    ///     The user is not logged in.
    /// </summary>
    NotLoggedIn,

    /// <summary>
    ///     The user does not have access to the content.
    /// </summary>
    AccessDenied,

    /// <summary>
    ///     The member account has not been approved.
    /// </summary>
    NotApproved,

    /// <summary>
    ///     The member account is locked out.
    /// </summary>
    LockedOut,

    /// <summary>
    ///     The user has access to the content.
    /// </summary>
    AccessAccepted,
}
