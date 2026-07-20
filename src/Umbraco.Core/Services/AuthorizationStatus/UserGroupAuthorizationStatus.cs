namespace Umbraco.Cms.Core.Services.AuthorizationStatus;

/// <summary>
///     Represents the result of a user group authorization check.
/// </summary>
public enum UserGroupAuthorizationStatus
{
    /// <summary>
    ///     The authorization check succeeded.
    /// </summary>
    Success,

    /// <summary>
    ///     The user does not have access to the allowed sections of the user group.
    /// </summary>
    UnauthorizedMissingAllowedSectionAccess,

    /// <summary>
    ///     The user does not have access to the content start node of the user group.
    /// </summary>
    UnauthorizedMissingContentStartNodeAccess,

    /// <summary>
    ///     The user does not have access to the media start node of the user group.
    /// </summary>
    UnauthorizedMissingMediaStartNodeAccess,

    /// <summary>
    ///     The user does not have access to the user group.
    /// </summary>
    UnauthorizedMissingUserGroupAccess,

    /// <summary>
    ///     The user does not have access to the Users section.
    /// </summary>
    UnauthorizedMissingUsersSectionAccess
}
