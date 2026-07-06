using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;

namespace Umbraco.Cms.Search.BackOffice.Services;

internal abstract class IndexedSearchServiceBase
{
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
