namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a stylesheet folder operation.
/// </summary>
public enum StylesheetFolderOperationStatus
{
    /// <summary>
    /// The stylesheet folder operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// A stylesheet folder with the same name already exists at the specified location.
    /// </summary>
    AlreadyExists,

    /// <summary>
    /// The specified stylesheet folder was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// The stylesheet folder cannot be deleted because it is not empty.
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
