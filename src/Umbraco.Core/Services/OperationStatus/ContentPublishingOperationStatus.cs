namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ContentPublishingOperationStatus
{
    Success,
    ContentNotFound,
    CancelledByEvent,
    ContentInvalid,
    NothingToPublish,
    MandatoryCultureMissing,
    HasExpired,
    CultureHasExpired,
    AwaitingRelease,
    CultureAwaitingRelease,
    InTrash,
    PathNotPublished,
    ConcurrencyViolation,
    UnsavedChanges,
    Failed, // unspecified failure (can happen on unpublish at the time of writing)
    Unknown
}
