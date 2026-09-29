namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an <see cref="IContentService" /> copy operation.
/// </summary>
public enum ContentCopyOperationStatus
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
    ///     The specified parent could not be found.
    /// </summary>
    ParentNotFound,
}
