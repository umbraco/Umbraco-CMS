namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ScriptOperationStatus
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
