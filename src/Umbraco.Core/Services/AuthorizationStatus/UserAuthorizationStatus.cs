namespace Umbraco.Cms.Core.Services.AuthorizationStatus;

/// <summary>
///     Represents the result of a user authorization check.
/// </summary>
public enum UserAuthorizationStatus
{
    /// <summary>
    ///     The authorization check succeeded.
    /// </summary>
    Success,

    /// <summary>
    ///     The user does not have administrator access.
    /// </summary>
    UnauthorizedMissingAdminAccess
}
