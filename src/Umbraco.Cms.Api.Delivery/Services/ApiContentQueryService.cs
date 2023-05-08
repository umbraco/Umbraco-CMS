using Examine;
using Examine.Search;
using Umbraco.Cms.Api.Delivery.Indexing.Selectors;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Api.Delivery.Services;

internal sealed class ApiContentQueryService : IApiContentQueryService // Examine-specific implementation - can be swapped out
{
    private const string ItemIdFieldName = "itemId";
    private readonly IExamineManager _examineManager;
    private readonly IRequestStartItemProviderAccessor _requestStartItemProviderAccessor;
    private readonly SelectorHandlerCollection _selectorHandlers;
    private readonly FilterHandlerCollection _filterHandlers;
    private readonly SortHandlerCollection _sortHandlers;
    private readonly string _fallbackGuidValue;

    public ApiContentQueryService(
        IExamineManager examineManager,
        IRequestStartItemProviderAccessor requestStartItemProviderAccessor,
        SelectorHandlerCollection selectorHandlers,
        FilterHandlerCollection filterHandlers,
        SortHandlerCollection sortHandlers)
    {
        _examineManager = examineManager;
        _requestStartItemProviderAccessor = requestStartItemProviderAccessor;
        _selectorHandlers = selectorHandlers;
        _filterHandlers = filterHandlers;
        _sortHandlers = sortHandlers;

        // A fallback value is needed for Examine queries in case we don't have a value - we can't pass null or empty string
        // It is set to a random guid since this would be highly unlikely to yield any results
        _fallbackGuidValue = Guid.NewGuid().ToString("D");
    }

    /// <inheritdoc/>
    public Attempt<PagedModel<Guid>, ApiContentQueryOperationStatus> ExecuteQuery(string? fetch, IEnumerable<string> filters, IEnumerable<string> sorts, int skip, int take)
    {
        var emptyResult = new PagedModel<Guid>();

        if (!_examineManager.TryGetIndex(Constants.UmbracoIndexes.DeliveryApiContentIndexName, out IIndex? apiIndex))
        {
            return Attempt.FailWithStatus(ApiContentQueryOperationStatus.IndexNotFound, emptyResult);
        }

        IQuery baseQuery = apiIndex.Searcher.CreateQuery();

        // Handle Selecting
        IBooleanOperation? queryOperation = HandleSelector(fetch, baseQuery);

        // If no Selector could be found, we return no results
        if (queryOperation is null)
        {
            return Attempt.FailWithStatus(ApiContentQueryOperationStatus.SelectorOptionNotFound, emptyResult);
        }

        // Handle Filtering
        var canApplyFiltering = CanHandleFiltering(filters, queryOperation);

        // If there is an invalid Filter option, we return no results
        if (canApplyFiltering is false)
        {
            return Attempt.FailWithStatus(ApiContentQueryOperationStatus.FilterOptionNotFound, emptyResult);
        }

        // Handle Sorting
        IOrdering? sortQuery = HandleSorting(sorts, queryOperation);

        // If there is an invalid Sort option, we return no results
        if (sortQuery is null)
        {
            return Attempt.FailWithStatus(ApiContentQueryOperationStatus.SortOptionNotFound, emptyResult);
        }

        ISearchResults? results = sortQuery
            .SelectField(ItemIdFieldName)
            .Execute(QueryOptions.SkipTake(skip, take));

        if (results is null)
        {
            // The query yield no results
            return Attempt.SucceedWithStatus(ApiContentQueryOperationStatus.Success, emptyResult);
        }

        Guid[] items = results
            .Where(r => r.Values.ContainsKey(ItemIdFieldName))
            .Select(r => Guid.Parse(r.Values[ItemIdFieldName]))
            .ToArray();

        return Attempt.SucceedWithStatus(ApiContentQueryOperationStatus.Success, new PagedModel<Guid>(results.TotalItemCount, items));
    }

    private IBooleanOperation? HandleSelector(string? fetch, IQuery baseQuery)
    {
        string? fieldName = null;
        string? fieldValue = null;

        if (fetch is not null)
        {
            ISelectorHandler? selectorHandler = _selectorHandlers.FirstOrDefault(h => h.CanHandle(fetch));
            SelectorOption? selector = selectorHandler?.BuildSelectorOption(fetch);

            if (selector is null)
            {
                return null;
            }

            fieldName = selector.FieldName;
            fieldValue = string.IsNullOrWhiteSpace(selector.Value) == false
                ? selector.Value
                : _fallbackGuidValue;
        }

        // Take into account the "start-item" header if present, as it defines a starting root node to query from
        if (fieldName is null && _requestStartItemProviderAccessor.TryGetValue(out IRequestStartItemProvider? requestStartItemProvider))
        {
            IPublishedContent? startItem = requestStartItemProvider.GetStartItem();
            if (startItem is not null)
            {
                // Reusing the boolean operation of the "Descendants" selector, as we want to get all the nodes from the given starting point
                fieldName = DescendantsSelectorIndexer.FieldName;
                fieldValue = startItem.Key.ToString();
            }
        }

        // If no params or no fetch value, get everything from the index - this is a way to do that with Examine
        fieldName ??= UmbracoExamineFieldNames.CategoryFieldName;
        fieldValue ??= "content";

        return baseQuery.Field(fieldName, fieldValue);
    }

    private bool CanHandleFiltering(IEnumerable<string> filters, IBooleanOperation queryOperation)
    {
        foreach (var filterValue in filters)
        {
            IFilterHandler? filterHandler = _filterHandlers.FirstOrDefault(h => h.CanHandle(filterValue));
            FilterOption? filter = filterHandler?.BuildFilterOption(filterValue);

            if (filter is null)
            {
                return false;
            }

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

        return true;
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
                return null;
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

        // Keep the index sorting as default
        return orderingQuery ?? queryCriteria.OrderBy();
    }
}
