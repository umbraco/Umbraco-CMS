using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core.Models;

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
    /// A collection of <see cref="ItemAncestorsResponseModel"/> containing the ancestor chain for each found entity.
    /// Ancestors are ordered root-first (ascending level). Entities not found are silently omitted.
    /// </returns>
    IEnumerable<ItemAncestorsResponseModel> GetAncestors(
        UmbracoObjectTypes itemObjectType,
        UmbracoObjectTypes? folderObjectType,
        ISet<Guid> entityKeys);
}
