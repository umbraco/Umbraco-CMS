namespace Umbraco.Cms.Core.Services;

/// <summary>
///     A value indicating the result of publishing or unpublishing a document.
/// </summary>
public enum PublishResultType : byte
{
    // all "ResultType" enum's must be byte-based, and declare Failed = 128, and declare
    // every failure codes as >128 - see OperationResult and OperationResultType for details.
    #region Success - Publish

    /// <summary>
    ///     The document was successfully published.
    /// </summary>
    SuccessPublish = 0,

    /// <summary>
    ///     The specified document culture was successfully published.
    /// </summary>
    SuccessPublishCulture = 1,

    /// <summary>
    ///     The document was already published.
    /// </summary>
    SuccessPublishAlready = 2,

    #endregion

    #region Success - Unpublish

    /// <summary>
    ///     The document was successfully unpublished.
    /// </summary>
    SuccessUnpublish = 3,

    /// <summary>
    ///     The document was already unpublished.
    /// </summary>
    SuccessUnpublishAlready = 4,

    /// <summary>
    ///     The specified document culture was unpublished, the document item itself remains published.
    /// </summary>
    SuccessUnpublishCulture = 5,

    /// <summary>
    ///     The specified document culture was unpublished, and was a mandatory culture, therefore the document itself was
    ///     unpublished.
    /// </summary>
    SuccessUnpublishMandatoryCulture = 6,

    /// <summary>
    ///     The specified document culture was unpublished, and was the last published culture in the document, therefore the
    ///     document itself was unpublished.
    /// </summary>
    SuccessUnpublishLastCulture = 8,

    #endregion

    #region Success - Mixed

    /// <summary>
    ///     Specified document cultures were successfully published and unpublished (in the same operation).
    /// </summary>
    SuccessMixedCulture = 7,

    #endregion

    #region Failed - Publish

    /// <summary>
    ///     The operation failed.
    /// </summary>
    /// <remarks>All values above this value indicate a failure.</remarks>
    FailedPublish = 128,

    /// <summary>
    ///     The document could not be published because its ancestor path is not published.
    /// </summary>
    FailedPublishPathNotPublished = FailedPublish | 1,

    /// <summary>
    ///     The document has expired so we cannot force it to be
    ///     published again as part of a bulk publish operation.
    /// </summary>
    FailedPublishHasExpired = FailedPublish | 2,

    /// <summary>
    ///     The document is scheduled to be released in the future and therefore we cannot force it to
    ///     be published during a bulk publish operation.
    /// </summary>
    FailedPublishAwaitingRelease = FailedPublish | 3,

    /// <summary>
    ///     A document culture has expired so we cannot force it to be
    ///     published again as part of a bulk publish operation.
    /// </summary>
    FailedPublishCultureHasExpired = FailedPublish | 4,

    /// <summary>
    ///     A document culture is scheduled to be released in the future and therefore we cannot force it to
    ///     be published during a bulk publish operation.
    /// </summary>
    FailedPublishCultureAwaitingRelease = FailedPublish | 5,

    /// <summary>
    ///     The document could not be published because it is in the trash.
    /// </summary>
    FailedPublishIsTrashed = FailedPublish | 6,

    /// <summary>
    ///     The publish action has been cancelled by an event handler.
    /// </summary>
    FailedPublishCancelledByEvent = FailedPublish | 7,

    /// <summary>
    ///     The document could not be published because it contains invalid data (has not passed validation requirements).
    /// </summary>
    FailedPublishContentInvalid = FailedPublish | 8,

    /// <summary>
    ///     The document could not be published because it has no publishing flags or values or if its a variant document, no
    ///     cultures were specified to be published.
    /// </summary>
    FailedPublishNothingToPublish = FailedPublish | 9,

    /// <summary>
    ///     The document could not be published because some mandatory cultures are missing.
    /// </summary>
    FailedPublishMandatoryCultureMissing = FailedPublish | 10, // in ContentService.SavePublishing

    /// <summary>
    ///     The document could not be published because it has been modified by another user.
    /// </summary>
    FailedPublishConcurrencyViolation = FailedPublish | 11,

    #endregion

    #region Failed - Unpublish

    /// <summary>
    ///     The document could not be unpublished.
    /// </summary>
    FailedUnpublish = FailedPublish | 11, // in ContentService.SavePublishing

    /// <summary>
    ///     The unpublish action has been cancelled by an event handler.
    /// </summary>
    FailedUnpublishCancelledByEvent = FailedPublish | 12,

    #endregion
}
