namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a dictionary item operation.
/// </summary>
public enum DictionaryItemOperationStatus
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
    ///     A dictionary item with the same key already exists.
    /// </summary>
    DuplicateItemKey,

    /// <summary>
    ///     The specified dictionary item was not found.
    /// </summary>
    ItemNotFound,

    /// <summary>
    ///     The specified parent dictionary item was not found.
    /// </summary>
    ParentNotFound,

    /// <summary>
    ///     The dictionary item ID is invalid.
    /// </summary>
    InvalidId,

    /// <summary>
    ///     A dictionary item with the same unique key already exists.
    /// </summary>
    DuplicateKey,

    /// <summary>
    ///     The specified parent dictionary item is invalid.
    /// </summary>
    InvalidParent
}
