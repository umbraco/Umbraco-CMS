namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum DataTypeContainerOperationStatus
{
    Success,
    CancelledByNotification,
    InvalidObjectType,
    InvalidId,
    InvalidKey,
    NotFound,
    ParentNotFound,
    NotEmpty,
    DuplicateName
}
