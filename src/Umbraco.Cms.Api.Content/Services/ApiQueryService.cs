using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.Search;
using Umbraco.Cms.Api.Content.Querying.Filters;
using Umbraco.Cms.Api.Content.Querying.Selectors;
using Umbraco.Cms.Api.Content.Querying.Sorts;
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

    public IEnumerable<Guid> ExecuteQuery(string? fetch, string[] filters, string[] sorts)
    {
        if (!_examineManager.TryGetIndex(Constants.UmbracoIndexes.ContentAPIIndexName, out IIndex? apiIndex))
        {
            return Enumerable.Empty<Guid>();
        }

        IQuery baseQuery = apiIndex.Searcher.CreateQuery();

        // Handle Selecting
        IBooleanOperation? queryOperation = HandleSelector(fetch, baseQuery);

        if (queryOperation is null)
        {
            return Enumerable.Empty<Guid>();
        }

        // Handle Filtering
        HandleFiltering(filters, baseQuery);

        // Handle Sorting
        var sortQuery = HandleSorting(sorts, queryOperation);

        //ISearchResults? results = sortQuery is not null ? sortQuery.Execute() : queryOperation.Execute();
        ISearchResults? results = sortQuery is not null ? sortQuery.Execute() : DefaultSort(queryOperation)?.Execute();

        return results!.Select(x => Guid.Parse(x.Id));
    }

    private IBooleanOperation? HandleSelector(string? fetch, IQuery baseQuery)
    {
        IBooleanOperation? queryOperation;

        if (fetch is not null &&
            _queryHandlers.FirstOrDefault(h => h.CanHandle(fetch)) is ISelectorHandler selectorHandler)
        {
            queryOperation = selectorHandler
                .BuildSelectorIndexQuery(baseQuery, fetch);
        }
        else
        {
            // TODO: If no params or no fetch value, get everything from the index
            queryOperation = baseQuery.Field("__IndexType", "content");
        }

        return queryOperation;
    }

    private void HandleFiltering(IEnumerable<string> filters, IQuery baseQuery)
    {
        foreach (var filter in filters)
        {
            if (_queryHandlers.FirstOrDefault(h => h.CanHandle(filter)) is IFilterHandler filterHandler)
            {
                filterHandler.BuildFilterIndexQuery(baseQuery, filter);
            }
        }
    }

    private IOrdering? HandleSorting(IEnumerable<string> sorts, IBooleanOperation queryCriteria)
    {
        IOrdering? orderingQuery = null;

        foreach (var sort in sorts)
        {
            if (_queryHandlers.FirstOrDefault(h => h.CanHandle(sort)) is ISortHandler sortHandler)
            {
                orderingQuery = sortHandler.BuildSortIndexQuery(queryCriteria, sort);
            }
        }

        return orderingQuery;
    }

    private IOrdering? DefaultSort(IBooleanOperation queryCriteria)
    {
        var defaultSorts = new[] { "path:asc", "sortOrder:asc" };

        return HandleSorting(defaultSorts, queryCriteria);
    }
}
