namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a content publishing operation.
/// </summary>
public enum ContentPublishingOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The specified content item was not found.
    /// </summary>
    ContentNotFound,

    /// <summary>
    ///     The operation was cancelled by an event handler.
    /// </summary>
    CancelledByEvent,

    /// <summary>
    ///     The content item is invalid and cannot be published.
    /// </summary>
    ContentInvalid,

    /// <summary>
    ///     There is nothing to publish for the content item.
    /// </summary>
    NothingToPublish,

    /// <summary>
    ///     A mandatory culture is missing from the content item.
    /// </summary>
    MandatoryCultureMissing,

    /// <summary>
    ///     The content item has expired and cannot be published.
    /// </summary>
    HasExpired,

    /// <summary>
    ///     The specified culture has expired and cannot be published.
    /// </summary>
    CultureHasExpired,

    /// <summary>
    ///     The content item is awaiting release and cannot be published immediately.
    /// </summary>
    AwaitingRelease,

    /// <summary>
    ///     The specified culture is awaiting release and cannot be published immediately.
    /// </summary>
    CultureAwaitingRelease,

    /// <summary>
    ///     The content item is in the recycle bin and cannot be published.
    /// </summary>
    InTrash,

    /// <summary>
    ///     The specified culture is invalid or not configured.
    /// </summary>
    InvalidCulture,

    /// <summary>
    ///     Cannot publish invariant content when the content type is configured as variant.
    /// </summary>
    CannotPublishInvariantWhenVariant,

    /// <summary>
    ///     Cannot publish variant content when the content type is not configured as variant.
    /// </summary>
    CannotPublishVariantWhenNotVariant,

    /// <summary>
    ///     The specified culture is missing from the content item.
    /// </summary>
    CultureMissing,

    /// <summary>
    ///     The content path is not published; parent content must be published first.
    /// </summary>
    PathNotPublished,

    /// <summary>
    ///     A concurrency violation occurred; the content was modified by another operation.
    /// </summary>
    ConcurrencyViolation,

    /// <summary>
    ///     The content item has unsaved changes that must be saved before publishing.
    /// </summary>
    UnsavedChanges,

    /// <summary>
    ///     The unpublish time must be after the publish time.
    /// </summary>
    UnpublishTimeNeedsToBeAfterPublishTime,

    /// <summary>
    ///     The unpublish time must be in the future.
    /// </summary>
    UpublishTimeNeedsToBeInFuture,

    /// <summary>
    ///     The publish time must be in the future.
    /// </summary>
    PublishTimeNeedsToBeInFuture,

    /// <summary>
    ///     The branch publishing operation failed.
    /// </summary>
    FailedBranch,

    /// <summary>
    ///     An unspecified failure occurred during the operation.
    /// </summary>
    // unspecified failure (can happen on unpublish at the time of writing)
    Failed,

    /// <summary>
    ///     An unknown error occurred during the operation.
    /// </summary>
    Unknown,

    /// <summary>
    ///     The content item cannot be unpublished because it is referenced by other items.
    /// </summary>
    CannotUnpublishWhenReferenced,

    /// <summary>
    ///     The publishing operation was accepted for processing.
    /// </summary>
    Accepted,

    /// <summary>
    ///     The task result for the publishing operation was not found.
    /// </summary>
    TaskResultNotFound,
}
