namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a webhook operation.
/// </summary>
public enum WebhookOperationStatus
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
    ///     The operation failed because the webhook could not be found.
    /// </summary>
    NotFound,

    /// <summary>
    ///     The operation failed because no events were specified for the webhook.
    /// </summary>
    NoEvents,
}
