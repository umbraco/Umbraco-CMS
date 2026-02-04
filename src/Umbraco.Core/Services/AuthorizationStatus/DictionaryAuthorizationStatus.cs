namespace Umbraco.Cms.Core.Services.AuthorizationStatus;

/// <summary>
///     Represents the result of a dictionary authorization check.
/// </summary>
public enum DictionaryAuthorizationStatus
{
    /// <summary>
    ///     The authorization check succeeded.
    /// </summary>
    Success,

    /// <summary>
    ///     The user does not have access to the specified culture.
    /// </summary>
    UnauthorizedMissingCulture
}
