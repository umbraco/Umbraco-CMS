namespace Umbraco.Cms.Api.Management.ViewModels.FileSystem;

/// <summary>
/// Represents the base view model for items in a file system, used within the API management context.
/// </summary>
public abstract class FileSystemItemViewModelBase
{
    /// <summary>
    /// Gets or sets the relative path of the file system item from the root of the file system.
    /// </summary>
    public required string Path { get; set; }
}
