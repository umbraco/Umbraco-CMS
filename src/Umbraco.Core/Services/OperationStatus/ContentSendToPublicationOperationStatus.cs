namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an <see cref="IContentService" /> send-to-publication operation.
/// </summary>
public enum ContentSendToPublicationOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    ///     No content was supplied to send to publication.
    /// </summary>
    NotFound,

    /// <summary>
    ///     Saving the content before sending it to publication failed.
    /// </summary>
    SaveFailed,
}
