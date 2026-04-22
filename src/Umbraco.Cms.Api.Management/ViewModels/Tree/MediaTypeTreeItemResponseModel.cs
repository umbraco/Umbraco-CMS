namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents a response model for a media type tree item in the Umbraco CMS Management API.
/// </summary>
public class MediaTypeTreeItemResponseModel : FolderTreeItemResponseModel
{
    /// <summary>
    /// Gets or sets the icon associated with the media type tree item.
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the media type tree item can be deleted.
    /// </summary>
    public bool IsDeletable { get; set; }
}
