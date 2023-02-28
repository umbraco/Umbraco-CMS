namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ContentEditingOperationStatus
{
    Success,
    CancelledByNotification,
    ContentTypeNotFound,
    ContentTypeCultureVarianceMismatch,
    NotFound,
    ParentNotFound,
    NotAllowed,
    TemplateNotFound,
    TemplateNotAllowed,
    PropertyTypeNotFound,
    Unknown
}
