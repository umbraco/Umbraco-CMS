namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an <see cref="IContentService" /> rollback operation.
/// </summary>
public enum ContentRollbackOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The content or the version to roll back to could not be found, or the content is trashed.
    /// </summary>
    ContentNotFound,

    /// <summary>
    ///     The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    ///     Saving the rolled-back content failed.
    /// </summary>
    SaveFailed,
}
