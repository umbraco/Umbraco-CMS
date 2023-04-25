using Examine;
using Examine.Search;
using Umbraco.Cms.Api.Delivery.Indexing.Sorts;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Api.Delivery.Services;

internal sealed class ApiContentQueryService : IApiContentQueryService // Examine-specific implementation - can be swapped out
{
    private readonly IExamineManager _examineManager;
    private readonly SelectorHandlerCollection _selectorHandlers;
    private readonly FilterHandlerCollection _filterHandlers;
    private readonly SortHandlerCollection _sortHandlers;
    private readonly string _fallbackGuidValue;

    public ApiContentQueryService(
        IExamineManager examineManager,
        SelectorHandlerCollection selectorHandlers,
        FilterHandlerCollection filterHandlers,
        SortHandlerCollection sortHandlers)
    {
        _examineManager = examineManager;
        _selectorHandlers = selectorHandlers;
        _filterHandlers = filterHandlers;
        _sortHandlers = sortHandlers;

        // A fallback value is needed for Examine queries in case we don't have a value - we can't pass null or empty string
        // It is set to a random guid since this would be highly unlikely to yield any results
        _fallbackGuidValue = Guid.NewGuid().ToString("D");
    }

    /// <inheritdoc/>
    public PagedModel<Guid> ExecuteQuery(string? fetch, IEnumerable<string> filters, IEnumerable<string> sorts, int skip, int take)
    {
        var emptyResult = new PagedModel<Guid>();

        if (!_examineManager.TryGetIndex(Constants.UmbracoIndexes.DeliveryApiContentIndexName, out IIndex? apiIndex))
        {
            return emptyResult;
        }

        IQuery baseQuery = apiIndex.Searcher.CreateQuery();

        // Handle Selecting
        IBooleanOperation? queryOperation = HandleSelector(fetch, baseQuery);

        // If no Selector could be found, we return no results
        if (queryOperation is null)
        {
            return emptyResult;
        }

        // Handle Filtering
        HandleFiltering(filters, queryOperation);

        // Handle Sorting
        IOrdering? sortQuery = HandleSorting(sorts, queryOperation);

        ISearchResults? results = (sortQuery ?? DefaultSort(queryOperation))?.Execute(QueryOptions.SkipTake(skip, take));

        if (results is null)
        {
            return emptyResult;
        }
        else
        {
            Guid[] items = results.Select(x => Guid.Parse(x.Id)).ToArray();
            return new PagedModel<Guid>(results.TotalItemCount, items);
        }
    }

    private IBooleanOperation? HandleSelector(string? fetch, IQuery baseQuery)
    {
        IBooleanOperation? queryOperation;

        if (fetch is not null)
        {
            ISelectorHandler? selectorHandler = _selectorHandlers.FirstOrDefault(h => h.CanHandle(fetch));
            SelectorOption? selector = selectorHandler?.BuildSelectorOption(fetch);

            if (selector is null)
            {
                return null;
            }

            var value = string.IsNullOrWhiteSpace(selector.Value) == false
                ? selector.Value
                : _fallbackGuidValue;
            queryOperation = baseQuery.Field(selector.FieldName, value);
        }
        else
        {
            // TODO: If no params or no fetch value, get everything from the index - make a default selector and register it by the end of the collection
            // TODO: This selects everything without regard to the current start-item header - make sure we honour that if it is present
            // This is a temp Examine solution
            queryOperation = baseQuery.Field("__IndexType", "content");
        }

        return queryOperation;
    }

    private void HandleFiltering(IEnumerable<string> filters, IBooleanOperation queryOperation)
    {
        foreach (var filterValue in filters)
        {
            IFilterHandler? filterHandler = _filterHandlers.FirstOrDefault(h => h.CanHandle(filterValue));
            FilterOption? filter = filterHandler?.BuildFilterOption(filterValue);

            if (filter is not null)
            {
                var value = string.IsNullOrWhiteSpace(filter.Value) == false
                    ? filter.Value
                    : _fallbackGuidValue;

                switch (filter.Operator)
                {
                    case FilterOperation.Is:
                        queryOperation.And().Field(filter.FieldName,
                            (IExamineValue)new ExamineValue(Examineness.Explicit,
                                value)); // TODO: doesn't work for explicit word(s) match
                        break;
                    case FilterOperation.IsNot:
                        queryOperation.Not().Field(filter.FieldName,
                            (IExamineValue)new ExamineValue(Examineness.Explicit,
                                value)); // TODO: doesn't work for explicit word(s) match
                        break;
                    // TODO: Fix
                    case FilterOperation.Contains:
                        break;
                    // TODO: Fix
                    case FilterOperation.DoesNotContain:
                        break;
                    default:
                        continue;
                }
            }
        }
    }

    private IOrdering? HandleSorting(IEnumerable<string> sorts, IBooleanOperation queryCriteria)
    {
        IOrdering? orderingQuery = null;

        foreach (var sortValue in sorts)
        {
            ISortHandler? sortHandler = _sortHandlers.FirstOrDefault(h => h.CanHandle(sortValue));
            SortOption? sort = sortHandler?.BuildSortOption(sortValue);

            if (sort is null)
            {
                continue;
            }

            SortType sortType = sort.FieldType switch
            {
                FieldType.Number => SortType.Int,
                FieldType.Date => SortType.Long,
                FieldType.String => SortType.String,
                FieldType.StringSortable => SortType.String,
                _ => throw new ArgumentOutOfRangeException(nameof(sort.FieldType))
            };

            orderingQuery = sort.Direction switch
            {
                Direction.Ascending => queryCriteria.OrderBy(new SortableField(sort.FieldName, sortType)),
                Direction.Descending => queryCriteria.OrderByDescending(new SortableField(sort.FieldName, sortType)),
                _ => orderingQuery
            };
        }

        return orderingQuery;
    }

    private IOrdering? DefaultSort(IBooleanOperation queryCriteria)
    {
        var defaultSorts = new[] { $"{PathSortIndexer.FieldName}:asc", $"{SortOrderSortIndexer.FieldName}:asc" };

        return HandleSorting(defaultSorts, queryCriteria);
    }
}
