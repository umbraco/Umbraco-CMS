namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum TemplateOperationStatus
{
    Success,
    CancelledByNotification,
    InvalidAlias,
    TemplateNotFound,
    MasterTemplateNotFound
}
