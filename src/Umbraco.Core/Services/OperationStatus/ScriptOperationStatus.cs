namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a script operation.
/// </summary>
public enum ScriptOperationStatus
{
    /// <summary>
    /// The script operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// A script with the same name already exists at the specified location.
    /// </summary>
    AlreadyExists,

    /// <summary>
    /// The operation was cancelled by a notification handler.
    /// </summary>
    CancelledByNotification,

    /// <summary>
    /// The file extension is not valid for a script file.
    /// </summary>
    InvalidFileExtension,

    /// <summary>
    /// The parent folder was not found.
    /// </summary>
    ParentNotFound,

    /// <summary>
    /// The resulting file path exceeds the maximum allowed length.
    /// </summary>
    PathTooLong,

    /// <summary>
    /// The provided script name is invalid.
    /// </summary>
    InvalidName,

    /// <summary>
    /// The specified script was not found.
    /// </summary>
    NotFound,
}
