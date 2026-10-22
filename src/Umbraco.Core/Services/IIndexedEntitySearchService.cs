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

    /// <summary>
    /// Searches for entities and returns the keys of the matching index documents in relevance order, without
    /// resolving them to entities.
    /// </summary>
    /// <remarks>
    /// Use this when an index may contain documents that cannot be resolved via <see cref="IEntityService"/>, and the
    /// caller resolves the keys itself.
    /// </remarks>
    /// <param name="objectType">The type of entities to search for.</param>
    /// <param name="query">The search query string.</param>
    /// <param name="parentId">Optional parent ID to filter results by.</param>
    /// <param name="contentTypeIds">Optional content type IDs to filter results by.</param>
    /// <param name="trashed">Optional filter for trashed (true) or non-trashed (false) entities.</param>
    /// <param name="culture">Optional culture to search in.</param>
    /// <param name="skip">The number of results to skip for pagination.</param>
    /// <param name="take">The maximum number of results to return.</param>
    /// <param name="ignoreUserStartNodes">If true, ignores user start node restrictions when searching.</param>
    /// <returns>A paged model containing the keys of the matching index documents.</returns>
    // TODO (V20): Remove the default implementation.
    async Task<PagedModel<Guid>> SearchKeysAsync(
        UmbracoObjectTypes objectType,
        string query,
        Guid? parentId,
        IEnumerable<Guid>? contentTypeIds,
        bool? trashed,
        string? culture = null,
        int skip = 0,
        int take = 100,
        bool ignoreUserStartNodes = false)
    {
        PagedModel<IEntitySlim> result = await SearchAsync(objectType, query, parentId, contentTypeIds, trashed, culture, skip, take, ignoreUserStartNodes);
        return new PagedModel<Guid> { Items = result.Items.Select(entity => entity.Key), Total = result.Total };
    }
}
