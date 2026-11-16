namespace Umbraco.Cms.Core.Services.AuthorizationStatus;

/// <summary>
///     Represents the result of a document blueprint authorization check.
/// </summary>
public enum DocumentBlueprintAuthorizationStatus
{
    /// <summary>
    ///     The authorization check succeeded.
    /// </summary>
    Success,

    /// <summary>
    ///     The document blueprint was not found.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The user does not have access to the document blueprint path.
    /// </summary>
    UnauthorizedMissingPathAccess,

    /// <summary>
    ///     The user does not have access to root-level document blueprints.
    /// </summary>
    UnauthorizedMissingRootAccess,
}
