namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a partial view operation.
/// </summary>
public enum PartialViewOperationStatus
{
    /// <summary>
    /// The partial view operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// A partial view with the same name already exists at the specified location.
    /// </summary>
    AlreadyExists,

    /// <summary>
    /// The parent folder was not found.
    /// </summary>
    ParentNotFound,

    /// <summary>
    /// The provided partial view name is invalid.
    /// </summary>
    InvalidName,

    /// <summary>
    /// The file extension is not valid for a partial view.
    /// </summary>
    InvalidFileExtension,

    /// <summary>
    /// The resulting file path exceeds the maximum allowed length.
    /// </summary>
    PathTooLong,

    /// <summary>
    /// The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    /// The specified partial view was not found.
    /// </summary>
    NotFound
}
