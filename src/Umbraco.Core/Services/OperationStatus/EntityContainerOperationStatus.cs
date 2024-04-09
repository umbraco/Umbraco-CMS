namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum EntityContainerOperationStatus
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
