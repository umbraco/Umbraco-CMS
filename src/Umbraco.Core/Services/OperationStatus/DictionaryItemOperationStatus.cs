namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum DictionaryItemOperationStatus
{
    Success,
    CancelledByNotification,
    DuplicateItemKey,
    ItemNotFound,
    ParentNotFound,
    InvalidId,
    DuplicateKey,
    InvalidParent
}
