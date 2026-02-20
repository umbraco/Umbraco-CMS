namespace Umbraco.Cms.Core.Models.FileSystem;

/// <summary>
///     Represents the base model for a folder in the file system.
/// </summary>
public abstract class FolderModelBase
{
    /// <summary>
    ///     Gets or sets the name of the folder.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the full path to the folder.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the path of the parent folder, or null if the folder is at the root.
    /// </summary>
    public string? ParentPath { get; set; }
}
