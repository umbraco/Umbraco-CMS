namespace Umbraco.Cms.Core.Models.FileSystem;

/// <summary>
///     Represents the base model for creating a new folder in the file system.
/// </summary>
public abstract class FolderCreateModel
{
    /// <summary>
    ///     Gets or sets the name of the folder to create.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Gets or sets the path of the parent folder, or null to create at the root.
    /// </summary>
    public string? ParentPath { get; set; }
}
