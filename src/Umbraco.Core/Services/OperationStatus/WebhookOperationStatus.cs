namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum WebhookOperationStatus
{
    Success,
    CancelledByNotification,
    NotFound,
    NoEvents,
}
