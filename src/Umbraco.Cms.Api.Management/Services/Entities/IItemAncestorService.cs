using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Services.Entities;

/// <summary>
/// Provides bulk ancestor lookups for entity items.
/// </summary>
public interface IItemAncestorService
{
    /// <summary>
    /// Gets the ancestor chains for a collection of entities identified by their keys.
    /// </summary>
    /// <param name="itemObjectType">The object type of the entities.</param>
    /// <param name="folderObjectType">The optional folder (container) object type, if the entity type supports folders.</param>
    /// <param name="entityKeys">The unique keys of the entities to retrieve ancestors for.</param>
    /// <returns>
    /// A collection of <see cref="ItemAncestorsResponseModel{NamedItemResponseModel}"/> containing the ancestor chain for each found entity.
    /// Ancestors are ordered root-first (ascending level). Entities not found are silently omitted.
    /// </returns>
    Task<IEnumerable<ItemAncestorsResponseModel<NamedItemResponseModel>>> GetAncestorsAsync(
        UmbracoObjectTypes itemObjectType,
        UmbracoObjectTypes? folderObjectType,
        ISet<Guid> entityKeys);

    /// <summary>
    /// Gets the ancestor chains for a collection of entities identified by their keys.
    /// </summary>
    /// <typeparam name="TAncestorItem">The ancestor item response model type.</typeparam>
    /// <param name="itemObjectType">The object type of the entities.</param>
    /// <param name="folderObjectType">The optional folder (container) object type, if the entity type supports folders.</param>
    /// <param name="entityKeys">The unique keys of the entities to retrieve ancestors for.</param>
    /// <param name="ancestorMapper">
    /// A delegate that receives the complete set of ancestor <see cref="IEntitySlim"/> entities
    /// and returns a corresponding set of ancestor response models.
    /// </param>
    /// <returns>
    /// A collection of <see cref="ItemAncestorsResponseModel{TAncestorItem}"/> containing the ancestor chain for each found entity.
    /// Ancestors are ordered root-first (ascending level). Entities not found are silently omitted.
    /// </returns>
    Task<IEnumerable<ItemAncestorsResponseModel<TAncestorItem>>> GetAncestorsAsync<TAncestorItem>(
        UmbracoObjectTypes itemObjectType,
        UmbracoObjectTypes? folderObjectType,
        ISet<Guid> entityKeys,
        Func<IEnumerable<IEntitySlim>, Task<IEnumerable<TAncestorItem>>> ancestorMapper)
        where TAncestorItem : ItemResponseModelBase;
}
