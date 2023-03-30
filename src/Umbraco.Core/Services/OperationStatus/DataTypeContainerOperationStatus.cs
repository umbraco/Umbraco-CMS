namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum DataTypeContainerOperationStatus
{
    Success,
    CancelledByNotification,
    InvalidObjectType,
    InvalidId,
    DuplicateKey,
    NotFound,
    ParentNotFound,
    NotEmpty,
    DuplicateName
}
