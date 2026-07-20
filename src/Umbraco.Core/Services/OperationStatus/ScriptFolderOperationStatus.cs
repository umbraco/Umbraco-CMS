namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a script folder operation.
/// </summary>
public enum ScriptFolderOperationStatus
{
    /// <summary>
    /// The script folder operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// A script folder with the same name already exists at the specified location.
    /// </summary>
    AlreadyExists,

    /// <summary>
    /// The specified script folder was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The script folder cannot be deleted because it is not empty.
    /// </summary>
    NotEmpty,

    /// <summary>
    /// The parent folder was not found.
    /// </summary>
    ParentNotFound,

    /// <summary>
    /// The provided folder name is invalid.
    /// </summary>
    InvalidName,
}
