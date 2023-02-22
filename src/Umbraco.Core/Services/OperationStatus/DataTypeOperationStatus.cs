namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum DataTypeOperationStatus
{
    Success,
    CancelledByNotification,
    InvalidConfiguration,
    InvalidName,
    InvalidId,
    NotFound,
    ParentNotFound
}
