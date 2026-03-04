namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a partial view folder operation.
/// </summary>
public enum PartialViewFolderOperationStatus
{
    /// <summary>
    /// The partial view folder operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// A partial view folder with the same name already exists at the specified location.
    /// </summary>
    AlreadyExists,

    /// <summary>
    /// The specified partial view folder was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The partial view folder cannot be deleted because it is not empty.
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
