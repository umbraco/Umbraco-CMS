namespace Umbraco.Cms.Api.Management.ViewModels.FileSystem;

/// <summary>
/// Represents the base response model for a file in the file system.
/// </summary>
public abstract class FileSystemFileResponseModelBase : FileSystemResponseModelBase
{
    /// <summary>
    /// Gets or sets the file's content as a string.
    /// </summary>
    public required string Content { get; set; }
}
