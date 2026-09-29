namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of an <see cref="IContentService" /> sort operation.
/// </summary>
public enum ContentSortOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     No items were supplied to sort, so no work was done.
    /// </summary>
    NoOperation,

    /// <summary>
    ///     The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,
}
