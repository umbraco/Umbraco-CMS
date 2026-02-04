namespace Umbraco.Cms.Core.Services.AuthorizationStatus;

/// <summary>
///     Represents the result of a content authorization check.
/// </summary>
public enum ContentAuthorizationStatus
{
    /// <summary>
    ///     The authorization check succeeded.
    /// </summary>
    Success,

    /// <summary>
    ///     The content item was not found.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The user does not have access to the recycle bin.
    /// </summary>
    UnauthorizedMissingBinAccess,

    /// <summary>
    ///     The user does not have access to one or more descendant items.
    /// </summary>
    UnauthorizedMissingDescendantAccess,

    /// <summary>
    ///     The user does not have access to the content path.
    /// </summary>
    UnauthorizedMissingPathAccess,

    /// <summary>
    ///     The user does not have access to root-level content.
    /// </summary>
    UnauthorizedMissingRootAccess,

    /// <summary>
    ///     The user does not have access to the specified culture.
    /// </summary>
    UnauthorizedMissingCulture,

    /// <summary>
    ///     The user does not have the required permission for this operation.
    /// </summary>
    UnauthorizedMissingPermissionAccess
}
