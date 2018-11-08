namespace Umbraco.Core.Services
{

    /// <summary>
    /// A value indicating the result of publishing a content item.
    /// </summary>
    public enum PublishResultType : byte
    {
        // all "ResultType" enums must be byte-based, and declare Failed = 128, and declare
        // every failure codes as >128 - see OperationResult and OperationResultType for details.

        #region Success - Publish
        /// <summary>
        /// The publishing was successful.
        /// </summary>
        SuccessPublish = 0,

        SuccessPublishCulture = 1,

        /// <summary>
        /// The item was already published.
        /// </summary>
        SuccessPublishAlready = 2,

        #endregion

        #region Success - Unpublish

        /// <summary>
        /// The unpublishing was successful.
        /// </summary>
        SuccessUnpublish = 3,

        /// <summary>
        /// The item was already unpublished.
        /// </summary>
        SuccessUnpublishAlready = 4,

        /// <summary>
        /// The specified variant was unpublished, the content item itself remains published.
        /// </summary>
        SuccessUnpublishCulture = 5,

        /// <summary>
        /// The specified variant was a mandatory culture therefore it was unpublished and the content item itself is unpublished
        /// </summary>
        SuccessUnpublishMandatoryCulture = 6,

        #endregion

        #region Success - Mixed

        /// <summary>
        /// A variant content item has a culture published and another culture unpublished in the same operation
        /// </summary>
        SuccessMixedCulture = 7,

        #endregion


        #region Failed - Publish

        /// <summary>
        /// The operation failed.
        /// </summary>
        /// <remarks>All values above this value indicate a failure.</remarks>
        FailedPublish = 128,

        /// <summary>
        /// The content could not be published because it's ancestor path isn't published.
        /// </summary>
        FailedPublishPathNotPublished = FailedPublish | 1,

        /// <summary>
        /// The content item was scheduled to be un-published and it has expired so we cannot force it to be
        /// published again as part of a bulk publish operation.
        /// </summary>
        FailedPublishHasExpired = FailedPublish | 2,

        /// <summary>
        /// The content item is scheduled to be released in the future and therefore we cannot force it to
        /// be published during a bulk publish operation.
        /// </summary>
        FailedPublishAwaitingRelease = FailedPublish | 3,

        /// <summary>
        /// A culture on the content item was scheduled to be un-published and it has expired so we cannot force it to be
        /// published again as part of a bulk publish operation.
        /// </summary>
        FailedPublishCultureHasExpired = FailedPublish | 4,

        /// <summary>
        /// A culture on the content item is scheduled to be released in the future and therefore we cannot force it to
        /// be published during a bulk publish operation.
        /// </summary>
        FailedPublishCultureAwaitingRelease = FailedPublish | 5,

        /// <summary>
        /// The content item could not be published because it is in the trash.
        /// </summary>
        FailedPublishIsTrashed = FailedPublish | 6,

        /// <summary>
        /// The publish action has been cancelled by an event handler.
        /// </summary>
        FailedPublishCancelledByEvent = FailedPublish | 7,

        /// <summary>
        /// The content item could not be published because it contains invalid data (has not passed validation requirements).
        /// </summary>
        FailedPublishContentInvalid = FailedPublish | 8,

        /// <summary>
        /// Cannot publish a document that has no publishing flags or values
        /// </summary>
        FailedPublishNothingToPublish = FailedPublish | 9, // in ContentService.StrategyCanPublish - fixme weird

        /// <summary>
        /// Some mandatory cultures are missing.
        /// </summary>
        FailedPublishMandatoryCultureMissing = FailedPublish | 10, // in ContentService.SavePublishing 

        #endregion

        #region Failed - Unpublish

        /// <summary>
        /// Unpublish failed
        /// </summary>
        FailedUnpublish = FailedPublish | 11, // in ContentService.SavePublishing

        /// <summary>
        /// The unpublish action has been cancelled by an event handler.
        /// </summary>
        FailedUnpublishCancelledByEvent = FailedPublish | 12,

        #endregion

    }
}
