namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a two-factor authentication operation.
/// </summary>
public enum TwoFactorOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The operation failed because the two-factor provider is already set up for the user.
    /// </summary>
    ProviderAlreadySetup,

    /// <summary>
    ///     The operation failed because the specified two-factor provider name could not be found.
    /// </summary>
    ProviderNameNotFound,

    /// <summary>
    ///     The operation failed because the verification code is invalid.
    /// </summary>
    InvalidCode,

    /// <summary>
    ///     The operation failed because the user could not be found.
    /// </summary>
    UserNotFound
}
