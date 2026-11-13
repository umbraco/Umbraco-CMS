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

    /// <summary>
    /// Gets or sets a value indicating whether the parent of this item is a folder.
    /// </summary>
    /// <remarks>
    /// Always <c>false</c> when the item has no parent.
    /// </remarks>
    // TODO (V20): Consider exposing this as Parent.IsFolder. This requires EntityTreeItemResponseModel.Parent to support a folder-aware reference type.
    public bool ParentIsFolder { get; set; }
}
