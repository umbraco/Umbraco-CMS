namespace Umbraco.Cms.Core.Services.OperationStatus;

/// <summary>
/// Represents the status of a package operation.
/// </summary>
public enum PackageOperationStatus
{
    /// <summary>
    /// The package operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The specified package was not found.
    /// </summary>
    NotFound,

    /// <summary>
    /// A package item with the same name already exists.
    /// </summary>
    DuplicateItemName,

    /// <summary>
    /// The provided package name is invalid.
    /// </summary>
    InvalidName
}
