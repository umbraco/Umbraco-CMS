namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an external login operation.
/// </summary>
public enum ExternalLoginOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The specified user was not found.
    /// </summary>
    UserNotFound,

    /// <summary>
    ///     An unknown error occurred during the operation.
    /// </summary>
    Unknown,

    /// <summary>
    ///     The external identity was not found.
    /// </summary>
    IdentityNotFound,

    /// <summary>
    ///     The authentication options for the provider were not found.
    /// </summary>
    AuthenticationOptionsNotFound,

    /// <summary>
    ///     Unlinking external logins is disabled for this provider.
    /// </summary>
    UnlinkingDisabled,

    /// <summary>
    ///     The provider key is invalid.
    /// </summary>
    InvalidProviderKey,

    /// <summary>
    ///     The authentication scheme was not found.
    /// </summary>
    AuthenticationSchemeNotFound,

    /// <summary>
    ///     The user is not authorized to perform this operation.
    /// </summary>
    Unauthorized,

    /// <summary>
    ///     The external login information was not found.
    /// </summary>
    ExternalInfoNotFound,

    /// <summary>
    ///     An identity operation failure occurred.
    /// </summary>
    IdentityFailure,

    /// <summary>
    ///     The user secret was not found.
    /// </summary>
    UserSecretNotFound,

    /// <summary>
    ///     The provided secret is invalid.
    /// </summary>
    InvalidSecret
}
