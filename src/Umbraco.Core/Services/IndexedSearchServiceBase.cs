using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Search.Querying.Filtering;
using Umbraco.Cms.Core.Search.Querying.Sorting;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides shared filter parsing and default sorting for indexed backoffice search services.
/// </summary>
public abstract class IndexedSearchServiceBase
{
    /// <summary>
    /// Gets the default sorter: descending relevance score.
    /// </summary>
    /// <returns>A sorter that orders results by descending relevance score.</returns>
    protected static Sorter DefaultSorter() => new ScoreSorter(Direction.Descending);

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
            filters.Add(new KeywordFilter(Umbraco.Cms.Core.Constants.IndexFieldNames.Id, [id.AsKeyword()], false));
            effectiveQuery = null;
        }

        if (parentId.HasValue)
        {
            filters.Add(new KeywordFilter(Umbraco.Cms.Core.Constants.IndexFieldNames.ParentId, [parentId.Value.AsKeyword()], false));
        }

        return filters;
    }
}
