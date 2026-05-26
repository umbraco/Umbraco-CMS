namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a redirect URL operation.
/// </summary>
public enum RedirectUrlOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    ///     The operation failed because the redirect URL could not be found.
    /// </summary>
    NotFound,
}
