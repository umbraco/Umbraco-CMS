namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents a response model for a document type tree item in the Umbraco CMS Management API.
/// </summary>
public class DocumentTypeTreeItemResponseModel : FolderTreeItemResponseModel
{
    /// <summary>
    /// Gets or sets a value indicating whether this document type is an element.
    /// </summary>
    public bool IsElement { get; set; }

    /// <summary>
    /// Gets or sets the icon associated with the document type tree item.
    /// </summary>
    public string Icon { get; set; } = string.Empty;
}
