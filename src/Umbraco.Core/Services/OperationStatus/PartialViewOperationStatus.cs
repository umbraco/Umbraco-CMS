namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum PartialViewOperationStatus
{
    Success,
    AlreadyExists,
    ParentNotFound,
    InvalidName,
    InvalidFileExtension,
    PathTooLong,
    CancelledByNotification,
    NotFound
}
