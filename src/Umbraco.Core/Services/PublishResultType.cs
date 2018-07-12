namespace Umbraco.Core.Services
{

    /// <summary>
    /// A value indicating the result of publishing a content item.
    /// </summary>
    public enum PublishResultType : byte
    {
        // all "ResultType" enums must be byte-based, and declare Failed = 128, and declare
        // every failure codes as >128 - see OperationResult and OperationResultType for details.

        /// <summary>
        /// The publishing was successful.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The item was already published.
        /// </summary>
        SuccessAlready = 1,

        /// <summary>
        /// The operation failed.
        /// </summary>
        /// <remarks>All values above this value indicate a failure.</remarks>
        Failed = 128,

        /// <summary>
        /// The content could not be published because it's ancestor path isn't published.
        /// </summary>
        FailedPathNotPublished = Failed | 1,

        /// <summary>
        /// The content item was scheduled to be un-published and it has expired so we cannot force it to be
        /// published again as part of a bulk publish operation.
        /// </summary>
        FailedHasExpired = Failed | 2,

        /// <summary>
        /// The content item is scheduled to be released in the future and therefore we cannot force it to
        /// be published during a bulk publish operation.
        /// </summary>
        FailedAwaitingRelease = Failed | 3,

        /// <summary>
        /// The content item could not be published because it is in the trash.
        /// </summary>
        FailedIsTrashed = Failed | 4,

        /// <summary>
        /// The publish action has been cancelled by an event handler.
        /// </summary>
        FailedCancelledByEvent = Failed | 5,

        /// <summary>
        /// The content item could not be published because it contains invalid data (has not passed validation requirements).
        /// </summary>
        FailedContentInvalid = Failed | 6,

        /// <summary>
        /// Cannot republish a document that hasn't been published.
        /// </summary>
        FailedNoPublishedValues = Failed | 7, // in ContentService.StrategyCanPublish - fixme weird

        /// <summary>
        /// Some mandatory cultures are missing, or are not valid.
        /// </summary>
        FailedCannotPublish = Failed | 8, // in ContentController.PublishInternal - fixme // FailedByCulture?

        /// <summary>
        /// Publishing changes triggered an unpublishing, due to missing mandatory cultures, and unpublishing failed.
        /// </summary>
        FailedToUnpublish = Failed | 9, // in ContentService.SavePublishing

        /// <summary>
        /// Some mandatory cultures are missing.
        /// </summary>
        FailedByCulture = Failed | 10, // in ContentService.SavePublishing
    }
}
