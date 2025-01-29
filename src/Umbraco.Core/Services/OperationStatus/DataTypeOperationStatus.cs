namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum DataTypeOperationStatus
{
    Success,
    CancelledByNotification,
    InvalidConfiguration,
    InvalidName,
    InvalidId,
    DuplicateKey,
    NotFound,
    ParentNotFound,
    ParentNotContainer,
    PropertyEditorNotFound,
    NonDeletable
}
