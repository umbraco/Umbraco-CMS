namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

/// <summary>
/// Represents the ancestors of an item in a tree.
/// </summary>
/// <typeparam name="TItem">The type of the tree item response model.</typeparam>
/// <remarks>
/// Endpoints return a tree-specific subclass, which gives the response its own OpenAPI schema name.
/// </remarks>
public abstract class TreeItemAncestorsResponseModel<TItem>
    where TItem : EntityTreeItemResponseModel
{
    /// <summary>
    /// Gets or sets the Id of the item the ancestors belong to.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the ancestors of the item, ordered from the root down. The item itself is not included.
    /// </summary>
    public IEnumerable<TItem> Ancestors { get; set; } = [];
}
