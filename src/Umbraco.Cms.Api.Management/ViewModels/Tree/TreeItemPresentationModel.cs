namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Presentation model for a tree item in the Umbraco CMS Management API.
/// </summary>
public class TreeItemPresentationModel : IHasChildren
{
    /// <inheritdoc />
    public bool HasChildren { get; set; }
}
