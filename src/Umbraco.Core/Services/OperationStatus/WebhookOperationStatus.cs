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

    /// <summary>
    ///     The operation failed because one or more of the webhook's headers has an invalid name or value,
    ///     or is a header that cannot be set on a webhook request.
    /// </summary>
    InvalidHeaders,
}
