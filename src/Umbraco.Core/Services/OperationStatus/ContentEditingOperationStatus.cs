namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ContentEditingOperationStatus
{
    Success,
    CancelledByNotification,
    ContentTypeNotFound,
    ContentTypeCultureVarianceMismatch,
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
    Unknown
}
