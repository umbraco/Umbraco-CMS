namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents a response model for a data type tree item in the Umbraco CMS Management API.
/// </summary>
public class DataTypeTreeItemResponseModel : FolderTreeItemResponseModel
{
    /// <summary>
    /// Gets or sets the alias of the editor UI associated with the data type.
    /// </summary>
    public string? EditorUiAlias { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this data type tree item is deletable.
    /// </summary>
    public bool IsDeletable { get; set; }
}
