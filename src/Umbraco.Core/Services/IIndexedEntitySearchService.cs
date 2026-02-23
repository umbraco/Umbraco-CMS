using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Performs entity search against search indexes.
/// </summary>
/// <remarks>
/// Note that this service only supports entity types that are included in search indexes.
/// By default this means documents, media and members.
/// </remarks>
public interface IIndexedEntitySearchService
{
    /// <summary>
    /// Searches for entities using the search index with support for filtering and pagination.
    /// </summary>
    /// <param name="objectType">The type of entities to search for.</param>
    /// <param name="query">The search query string.</param>
    /// <param name="parentId">Optional parent ID to restrict the search to descendants of a specific node.</param>
    /// <param name="contentTypeIds">Optional collection of content type IDs to filter results by.</param>
    /// <param name="trashed">Optional filter for trashed state. If null, all items are included.</param>
    /// <param name="culture">Optional culture code to filter results by language variant.</param>
    /// <param name="skip">The number of results to skip for pagination.</param>
    /// <param name="take">The maximum number of results to return.</param>
    /// <param name="ignoreUserStartNodes">If true, ignores user start node restrictions when searching.</param>
    /// <returns>A paged model containing the matching entities.</returns>
    Task<PagedModel<IEntitySlim>> SearchAsync(
        UmbracoObjectTypes objectType,
        string query,
        Guid? parentId,
        IEnumerable<Guid>? contentTypeIds,
        bool? trashed,
        string? culture = null,
        int skip = 0,
        int take = 100,
        bool ignoreUserStartNodes = false);
}
