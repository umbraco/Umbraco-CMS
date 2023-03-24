using Examine;
using Examine.Search;
using Umbraco.Cms.Api.Content.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Services;
internal sealed class ApiQueryService : IApiQueryService
{
    private readonly QueryHandlerCollection _queryHandlers;
    private readonly IExamineManager _examineManager;

    public ApiQueryService(QueryHandlerCollection queryHandlers, IExamineManager examineManager)
    {
        _queryHandlers = queryHandlers;
        _examineManager = examineManager;
    }

    public IEnumerable<Guid> ExecuteQuery(string? fetch, string[]? filter, string[]? sort)
    {
        var queryHandler = fetch is not null ? _queryHandlers.FirstOrDefault(h => h.CanHandle(fetch)) as ISelectorHandler : null;

        // TODO: If no handler, get everything from the index
        if (queryHandler is null)
        {
            return Enumerable.Empty<Guid>();
        }

        if (!_examineManager.TryGetIndex(Constants.UmbracoIndexes.ContentAPIIndexName, out IIndex? apiIndex))
        {
            return Enumerable.Empty<Guid>();
        }

        IQuery baseQuery = apiIndex.Searcher.CreateQuery();
        IBooleanOperation? queryOperation = queryHandler
            .BuildApiIndexQuery(baseQuery, fetch!);

        if (queryOperation is null)
        {
            return Enumerable.Empty<Guid>();
        }

        // if (filter is not null)
        // {
        //     var alias = GetContentTypeAliasFromFilter(filter);
        //
        //     if (alias is not null)
        //     {
        //         queryOperation = queryOperation
        //             .And()
        //             .Field("__NodeTypeAlias", alias); // use that for now
        //     }
        // }

        ISearchResults results = queryOperation.Execute();

        return results.Select(x => Guid.Parse(x.Id));
    }

    private string? GetContentTypeAliasFromFilter(string filterValue)
    {
        if (!filterValue.StartsWith("contentType", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return filterValue.Substring(filterValue.IndexOf(':', StringComparison.Ordinal) + 1);
    }
}
