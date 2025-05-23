namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum StylesheetOperationStatus
{
    Success,
    AlreadyExists,
    CancelledByNotification,
    InvalidFileExtension,
    ParentNotFound,
    PathTooLong,
    InvalidName,
    NotFound,
}
