namespace Umbraco.Cms.Core.Services.AuthorizationStatus;

/// <summary>
///     Represents the result of a media authorization check.
/// </summary>
public enum MediaAuthorizationStatus
{
    /// <summary>
    ///     The authorization check succeeded.
    /// </summary>
    Success,

    /// <summary>
    ///     The media item was not found.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The user does not have access to the recycle bin.
    /// </summary>
    UnauthorizedMissingBinAccess,

    /// <summary>
    ///     The user does not have access to the media path.
    /// </summary>
    UnauthorizedMissingPathAccess,

    /// <summary>
    ///     The user does not have access to root-level media.
    /// </summary>
    UnauthorizedMissingRootAccess
}
