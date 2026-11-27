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

    /// <summary>
    ///     The content's key cannot be used by the kind's persistence, for example a key the kind's path scheme
    ///     cannot use.
    /// </summary>
    InvalidKey,

    /// <summary>
    ///     The content is new but its key already belongs to an existing item of the kind.
    /// </summary>
    DuplicateKey,
}
