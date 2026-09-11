// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for <see cref="PublishResult" />.
/// </summary>
public static class PublishResultExtensions
{
    /// <summary>
    ///     Gets the publishing operation status corresponding to a publish result.
    /// </summary>
    /// <param name="publishResult">The publish result.</param>
    /// <returns>The corresponding <see cref="ContentPublishingOperationStatus" />.</returns>
    public static ContentPublishingOperationStatus ToContentPublishingOperationStatus(this PublishResult publishResult)
        => publishResult.Result switch
        {
            PublishResultType.SuccessPublish => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessPublishCulture => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessPublishAlready => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessUnpublish => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessUnpublishAlready => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessUnpublishCulture => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessUnpublishMandatoryCulture => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessUnpublishLastCulture => ContentPublishingOperationStatus.Success,
            PublishResultType.SuccessMixedCulture => ContentPublishingOperationStatus.Success,
            PublishResultType.FailedPublish => ContentPublishingOperationStatus.Failed,
            PublishResultType.FailedPublishPathNotPublished => ContentPublishingOperationStatus.PathNotPublished,
            PublishResultType.FailedPublishHasExpired => ContentPublishingOperationStatus.HasExpired,
            PublishResultType.FailedPublishAwaitingRelease => ContentPublishingOperationStatus.AwaitingRelease,
            PublishResultType.FailedPublishCultureHasExpired => ContentPublishingOperationStatus.CultureHasExpired,
            PublishResultType.FailedPublishCultureAwaitingRelease => ContentPublishingOperationStatus.CultureAwaitingRelease,
            PublishResultType.FailedPublishIsTrashed => ContentPublishingOperationStatus.InTrash,
            PublishResultType.FailedPublishCancelledByEvent => ContentPublishingOperationStatus.CancelledByEvent,
            PublishResultType.FailedPublishContentInvalid => ContentPublishingOperationStatus.ContentInvalid,
            PublishResultType.FailedPublishNothingToPublish => ContentPublishingOperationStatus.NothingToPublish,
            PublishResultType.FailedPublishMandatoryCultureMissing => ContentPublishingOperationStatus.MandatoryCultureMissing,
            PublishResultType.FailedPublishConcurrencyViolation => ContentPublishingOperationStatus.ConcurrencyViolation,
            PublishResultType.FailedPublishUnsavedChanges => ContentPublishingOperationStatus.UnsavedChanges,
            PublishResultType.FailedUnpublish => ContentPublishingOperationStatus.Failed,
            PublishResultType.FailedUnpublishCancelledByEvent => ContentPublishingOperationStatus.CancelledByEvent,
            _ => ContentPublishingOperationStatus.Unknown,
        };
}
