namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum TemplateOperationStatus
{
    Success,
    CancelledByNotification,
    InvalidAlias,
    DuplicateAlias,
    TemplateNotFound,
    MasterTemplateNotFound,
    CircularMasterTemplateReference,
    MasterTemplateCannotBeDeleted,
}
