﻿namespace Umbraco.Cms.Core.Services.OperationStatus;

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
    InvalidCulture,
    CultureMissing,
    PathNotPublished,
    ConcurrencyViolation,
    UnsavedChanges,
    UnpublishTimeNeedsToBeAfterPublishTime,
    UpublishTimeNeedsToBeInFuture,
    PublishTimeNeedsToBeInFuture,
    FailedBranch,
    Failed, // unspecified failure (can happen on unpublish at the time of writing)
    Unknown,
}
