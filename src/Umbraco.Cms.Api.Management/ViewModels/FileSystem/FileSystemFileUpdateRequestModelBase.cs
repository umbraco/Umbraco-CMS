namespace Umbraco.Cms.Api.Management.ViewModels.FileSystem;

/// <summary>
/// Serves as the base class for models used to update files within the file system.
/// </summary>
public abstract class FileSystemFileUpdateRequestModelBase
{
    /// <summary>
    /// Gets or sets the content of the file.
    /// </summary>
    public required string Content { get; set; }
}
