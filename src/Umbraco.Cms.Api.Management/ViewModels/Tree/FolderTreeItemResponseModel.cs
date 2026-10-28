using System.Text.Json.Serialization;

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
    [JsonIgnore]
    [Obsolete("This property is temporary. Parent will become settable including whether it is a folder. Scheduled for removal in Umbraco 21.")]
    public bool ParentIsFolder { get; set; }

    /// <summary>
    /// Gets a reference to the parent, including whether it is a folder, or <c>null</c> if there is no parent.
    /// </summary>
    // TODO (V21): Replace with a plain settable Parent of this type and remove ParentIsFolder.
    public new FolderTreeItemParentReferenceModel? Parent
        => base.Parent is null
            ? null
#pragma warning disable CS0618 // Type or member is obsolete
            : new FolderTreeItemParentReferenceModel { Id = base.Parent.Id, IsFolder = ParentIsFolder };
#pragma warning restore CS0618 // Type or member is obsolete
}
