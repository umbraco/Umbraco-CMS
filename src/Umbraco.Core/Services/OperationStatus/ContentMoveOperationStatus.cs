namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an <see cref="IContentService" /> move operation.
/// </summary>
public enum ContentMoveOperationStatus
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
    ///     The specified parent was not found.
    /// </summary>
    ParentNotFound,

    /// <summary>
    ///     The specified parent is in the recycle bin.
    /// </summary>
    ParentTrashed,
}
