namespace Umbraco.Cms.Api.Management.ViewModels.FileSystem;

/// <summary>
/// Serves as the base request model for creating files within the file system via the API.
/// </summary>
public abstract class FileSystemFileCreateRequestModelBase : FileSystemCreateRequestModelBase
{
    /// <summary>
    /// Gets or sets the content of the file to be created.
    /// </summary>
    public required string Content { get; set; }
}
