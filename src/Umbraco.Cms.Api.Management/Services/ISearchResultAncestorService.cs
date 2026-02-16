using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Resolves ancestor (breadcrumb) details for search result entities in a single batch operation.
/// </summary>
public interface ISearchResultAncestorService
{
    /// <summary>
    /// Resolves the ancestors for a collection of tree entities, returning an ordered breadcrumb trail per entity.
    /// </summary>
    /// <param name="entities">The search result entities to resolve ancestors for.</param>
    /// <param name="entityObjectType">The <see cref="UmbracoObjectTypes"/> of the entities being resolved.</param>
    /// <returns>
    /// A dictionary keyed by entity key, where each value is an ordered list of ancestor models
    /// from the topmost ancestor to the immediate parent. Flat entity types (e.g. Member) return empty lists.
    /// </returns>
    Task<IReadOnlyDictionary<Guid, IReadOnlyList<SearchResultAncestorModel>>> ResolveAsync(
        IEnumerable<ITreeEntity> entities,
        UmbracoObjectTypes entityObjectType);
}
