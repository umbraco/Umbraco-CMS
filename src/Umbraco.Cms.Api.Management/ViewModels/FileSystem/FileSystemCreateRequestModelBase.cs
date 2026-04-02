namespace Umbraco.Cms.Api.Management.ViewModels.FileSystem;

/// <summary>
/// Serves as the base class for models used to create file system configurations in the API.
/// </summary>
public abstract class FileSystemCreateRequestModelBase
{
    /// <summary>
    /// Gets or sets the name of the file system.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the parent folder of the file system item to be created.
    /// </summary>
    public FileSystemFolderModel? Parent { get; set; }
}
