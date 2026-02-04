namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
///     Represents the status of a stylesheet operation.
/// </summary>
public enum StylesheetOperationStatus
{
    /// <summary>
    ///     The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    ///     The operation failed because a stylesheet with the same path already exists.
    /// </summary>
    AlreadyExists,

    /// <summary>
    ///     The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    ///     The operation failed because the file extension is not valid for a stylesheet.
    /// </summary>
    InvalidFileExtension,

    /// <summary>
    ///     The operation failed because the parent folder could not be found.
    /// </summary>
    ParentNotFound,

    /// <summary>
    ///     The operation failed because the stylesheet path exceeds the maximum allowed length.
    /// </summary>
    PathTooLong,

    /// <summary>
    ///     The operation failed because the stylesheet name is invalid.
    /// </summary>
    InvalidName,

    /// <summary>
    ///     The operation failed because the stylesheet could not be found.
    /// </summary>
    NotFound,
}
