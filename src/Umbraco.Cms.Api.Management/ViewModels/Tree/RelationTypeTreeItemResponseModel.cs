namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents a response model for a relation type tree item in the Umbraco CMS Management API.
/// </summary>
public class RelationTypeTreeItemResponseModel : NamedEntityTreeItemResponseModel
{
    /// <summary>
    /// Gets or sets a value indicating whether this relation type tree item can be deleted.
    /// </summary>
    public bool IsDeletable { get; set; }
}
