namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents a content tree item returned by the Umbraco CMS Management API.
/// </summary>
public abstract class ContentTreeItemResponseModel : EntityTreeItemResponseModel
{
    /// <summary>
    /// Gets or sets a value indicating whether the user does not have access to this content item.
    /// </summary>
    public bool NoAccess { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the content item is trashed.
    /// </summary>
    public bool IsTrashed { get; set; }

    /// <summary>
    /// Gets or sets the creation date of the content item.
    /// </summary>
    public DateTimeOffset CreateDate { get; set; }
}
