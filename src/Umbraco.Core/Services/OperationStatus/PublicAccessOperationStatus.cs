namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum PublicAccessOperationStatus
{
    Success,
    ContentNotFound,
    LoginNodeNotFound,
    ErrorNodeNotFound,
    NoAllowedEntities,
    AmbiguousRule,
    CancelledByNotification,
    EntryNotFound,
}
