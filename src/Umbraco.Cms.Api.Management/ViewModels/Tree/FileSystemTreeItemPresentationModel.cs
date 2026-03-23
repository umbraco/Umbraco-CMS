using Umbraco.Cms.Api.Management.ViewModels.FileSystem;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents the view model for an item in the file system tree within the management API.
/// </summary>
public class FileSystemTreeItemPresentationModel : TreeItemPresentationModel
{
    /// <summary>
    /// Gets or sets the name of the file system tree item.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the relative path of the file system tree item from the root of the file system tree.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parent folder of this file system tree item.
    /// </summary>
    public FileSystemFolderModel? Parent { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this item is a folder.
    /// </summary>
    public bool IsFolder { get; set; }
}
