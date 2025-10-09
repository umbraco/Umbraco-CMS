namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ContentEditingOperationStatus
{
    Success,
    CancelledByNotification,
    ContentTypeNotFound,
    ContentTypeCultureVarianceMismatch,
    ContentTypeSegmentVarianceMismatch,
    NotFound,
    ParentNotFound,
    ParentInvalid,
    NotAllowed,
    TemplateNotFound,
    TemplateNotAllowed,
    PropertyTypeNotFound,
    InTrash,
    NotInTrash,
    SortingInvalid,
    PropertyValidationError,
    InvalidCulture,
    DuplicateKey,
    DuplicateName,
    Unknown,
    CannotDeleteWhenReferenced,
    CannotMoveToRecycleBinWhenReferenced,
}
