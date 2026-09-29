namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an <see cref="IContentService" /> move-to-recycle-bin operation.
/// </summary>
public enum ContentMoveToRecycleBinOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,
}
