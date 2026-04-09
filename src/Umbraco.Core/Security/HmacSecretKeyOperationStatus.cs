namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Represents the status of an HMAC secret key operation.
/// </summary>
public enum HmacSecretKeyOperationStatus
{
    /// <summary>
    ///     The key was successfully generated and persisted to configuration.
    /// </summary>
    Success,

    /// <summary>
    ///     A key already exists; no new key was generated.
    /// </summary>
    KeyExists,

    /// <summary>
    ///     The key could not be persisted to configuration.
    /// </summary>
    Error,
}
