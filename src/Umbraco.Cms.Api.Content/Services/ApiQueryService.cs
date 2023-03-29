using Examine;
using Examine.Search;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ContentApi;
using SortType = Examine.Search.SortType;

namespace Umbraco.Cms.Api.Content.Services;

internal sealed class ApiQueryService : IApiQueryService
{
    private readonly IExamineManager _examineManager;
    private readonly SelectorHandlerCollection _selectorHandlers;
    private readonly FilterHandlerCollection _filterHandlers;
    private readonly SortHandlerCollection _sortHandlers;

    public ApiQueryService(
        IExamineManager examineManager,
        SelectorHandlerCollection selectorHandlers,
        FilterHandlerCollection filterHandlers,
        SortHandlerCollection sortHandlers)
    {
        _examineManager = examineManager;
        _selectorHandlers = selectorHandlers;
        _filterHandlers = filterHandlers;
        _sortHandlers = sortHandlers;
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
        HandleFiltering(filters, queryOperation);

        // Handle Sorting
        IOrdering? sortQuery = HandleSorting(sorts, queryOperation);

        //ISearchResults? results = sortQuery is not null ? sortQuery.Execute() : queryOperation.Execute();
        ISearchResults? results = sortQuery is not null ? sortQuery.Execute() : DefaultSort(queryOperation)?.Execute();

        return results!.Select(x => Guid.Parse(x.Id));
    }

    private IBooleanOperation? HandleSelector(string? fetch, IQuery baseQuery)
    {
        IBooleanOperation? queryOperation;

        if (fetch is not null)
        {
            ISelectorHandler? selectorHandler = _selectorHandlers.FirstOrDefault(h => h.CanHandle(fetch));
            SelectorOption? selector = selectorHandler?.BuildSelectorOption(fetch);
            queryOperation = selector is not null ? baseQuery.Field(selector.FieldName, selector.Value) : null;
        }
        else
        {
            // TODO: If no params or no fetch value, get everything from the index
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
                switch (filter.Operator)
                {
                    case FilterOperation.Is:
                        //queryOperation.And().Field(filter.FieldName, filter.Value);
                        queryOperation.And().Field(filter.FieldName,
                            (IExamineValue)new ExamineValue(Examineness.Explicit, filter.Value)); // doesn't work for explicit word(s) match
                        break;
                    case FilterOperation.IsNot:
                        //queryOperation.Not().Field(filter.FieldName, filter.Value);
                        queryOperation.Not().Field(filter.FieldName,
                            (IExamineValue)new ExamineValue(Examineness.Explicit, filter.Value)); // doesn't work for explicit word(s) match
                        break;
                    case FilterOperation.Contains:
                        break;
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

            SortType sortType = Enum.Parse<SortType>(sort.SortType.ToString());

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
        var defaultSorts = new[] { "path:asc", "sortOrder:asc" };

        return HandleSorting(defaultSorts, queryCriteria);
    }
}
