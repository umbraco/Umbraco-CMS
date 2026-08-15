using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;

namespace Umbraco.Cms.Search.BackOffice.Services;

/// <summary>
/// Provides shared filter parsing for indexed backoffice search services.
/// </summary>
internal abstract class IndexedSearchServiceBase
{
    /// <summary>
    /// Builds the filters for a search query, extracting an ID or parent ID filter where applicable.
    /// </summary>
    /// <param name="query">The search query. If it parses as a GUID, an ID filter is used instead of a text query.</param>
    /// <param name="parentId">Optional parent ID to filter results by.</param>
    /// <param name="effectiveQuery">The query to actually run as a full text search, or null if it was consumed as an ID filter.</param>
    /// <returns>The filters derived from <paramref name="query"/> and <paramref name="parentId"/>.</returns>
    protected List<Filter> ParseFilters(string? query, Guid? parentId, out string? effectiveQuery)
    {
        effectiveQuery = query;

        var filters = new List<Filter>();

        if (Guid.TryParse(query, out Guid id))
        {
            // if the query is an ID, filter for that ID rather than attempting a full text query match (which will likely turn up empty)
            filters.Add(new KeywordFilter(Core.Constants.FieldNames.Id, [id.AsKeyword()], false));
            effectiveQuery = null;
        }

        if (parentId.HasValue)
        {
            filters.Add(new KeywordFilter(Core.Constants.FieldNames.ParentId, [parentId.Value.AsKeyword()], false));
        }

        return filters;
    }
}
