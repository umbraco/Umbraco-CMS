namespace Umbraco.Cms.Api.Management.ViewModels.FileSystem;

/// <summary>
/// Serves as the base class for response models related to file system operations in the API.
/// </summary>
public abstract class FileSystemResponseModelBase : FileSystemItemViewModelBase
{
    /// <summary>
    /// Gets or sets the name of the file or directory represented by this file system item.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the parent folder of the current file system item.
    /// </summary>
    public FileSystemFolderModel? Parent { get; set; }
}
