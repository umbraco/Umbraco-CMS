namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents a folder item within a tree view response in the management API.
/// </summary>
public class FolderTreeItemResponseModel : NamedEntityTreeItemResponseModel
{
    /// <summary>
    /// Gets or sets a value indicating whether this item is a folder.
    /// </summary>
    public bool IsFolder { get; set; }
}
