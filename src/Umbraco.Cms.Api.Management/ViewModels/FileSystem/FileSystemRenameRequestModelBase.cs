namespace Umbraco.Cms.Api.Management.ViewModels.FileSystem;

/// <summary>
/// Represents the base model for renaming a file system entity.
/// </summary>
public abstract class FileSystemRenameRequestModelBase
{
    /// <summary>
    /// Gets or sets the new name for the file system item.
    /// </summary>
    public required string Name { get; set; }
}
