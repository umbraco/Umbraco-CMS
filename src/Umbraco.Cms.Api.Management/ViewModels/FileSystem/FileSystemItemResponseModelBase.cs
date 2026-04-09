namespace Umbraco.Cms.Api.Management.ViewModels.FileSystem;

/// <summary>
/// Serves as the base class for response models representing items within a file system.
/// </summary>
public abstract class FileSystemItemResponseModelBase : FileSystemResponseModelBase
{
    /// <summary>
    /// Gets or sets a value indicating whether the item is a folder.
    /// </summary>
    public required bool IsFolder { get; set; }
}
