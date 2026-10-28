namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents a reference to the parent of an item in a tree that supports folders.
/// </summary>
public sealed class FolderTreeItemParentReferenceModel
{
    /// <summary>
    /// Gets or sets the unique identifier of the parent.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the parent is a folder.
    /// </summary>
    public bool IsFolder { get; set; }
}
