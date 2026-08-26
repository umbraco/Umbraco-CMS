namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an <see cref="IContentService" /> save operation.
/// </summary>
public enum ContentSaveOperationStatus
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
    ///     The content's name exceeds the maximum allowed length (255 characters).
    /// </summary>
    InvalidName,

    /// <summary>
    ///     The content is in a transitional publishing/unpublishing state and cannot be saved directly;
    ///     use the dedicated publish method instead.
    /// </summary>
    InvalidPublishedState,
}
