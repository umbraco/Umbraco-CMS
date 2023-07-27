namespace Umbraco.Cms.Core.Services.OperationStatus;

public enum ContentPublishingOperationStatus
{
    Success,
    SuccessPublishCulture,
    ContentNotFound,
    FailedCancelledByEvent,
    FailedContentInvalid,
    FailedNothingToPublish,
    FailedMandatoryCultureMissing,
    FailedHasExpired,
    FailedCultureHasExpired,
    FailedAwaitingRelease,
    FailedCultureAwaitingRelease,
    FailedIsTrashed,
    FailedPathNotPublished
}
