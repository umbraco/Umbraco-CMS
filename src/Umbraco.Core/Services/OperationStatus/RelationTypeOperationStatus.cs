namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum RelationTypeOperationStatus
{
    Success,
    NotFound,
    KeyAlreadyExists,
    CancelledByNotification,
    InvalidId,
    InvalidChildObjectType,
    InvalidParentObjectType,
}
