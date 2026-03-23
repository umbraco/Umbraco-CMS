namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents a response model for a member type tree item in the Umbraco CMS Management API.
/// </summary>
public class MemberTypeTreeItemResponseModel : FolderTreeItemResponseModel
{
    /// <summary>
    /// Gets or sets the icon associated with the member type tree item.
    /// </summary>
    public string Icon { get; set; } = string.Empty;
}
