namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a user client credentials operation.
/// </summary>
public enum UserClientCredentialsOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The operation failed because a client with the same ID already exists.
    /// </summary>
    DuplicateClientId,

    /// <summary>
    ///     The operation failed because the user is invalid or does not exist.
    /// </summary>
    InvalidUser,

    /// <summary>
    ///     The operation failed because the client ID is invalid.
    /// </summary>
    InvalidClientId
}
