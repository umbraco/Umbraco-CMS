using Examine;
using Examine.Search;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Delivery.Indexing.Selectors;
using Umbraco.Cms.Api.Delivery.Indexing.Sorts;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Api.Delivery.Services;

internal sealed class ApiContentQueryService : IApiContentQueryService
{
    private readonly IExamineManager _examineManager;
    private readonly IRequestStartItemProviderAccessor _requestStartItemProviderAccessor;
    private readonly SelectorHandlerCollection _selectorHandlers;
    private readonly FilterHandlerCollection _filterHandlers;
    private readonly SortHandlerCollection _sortHandlers;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly ILogger<ApiContentQueryService> _logger;
    private readonly string _fallbackGuidValue;
    private readonly ISet<string> _itemIdOnlyFieldSet = new HashSet<string> { "itemId" };
    private readonly Dictionary<string, FieldType> _fieldTypes;

    public ApiContentQueryService(
        IExamineManager examineManager,
        IRequestStartItemProviderAccessor requestStartItemProviderAccessor,
        SelectorHandlerCollection selectorHandlers,
        FilterHandlerCollection filterHandlers,
        SortHandlerCollection sortHandlers,
        ContentIndexHandlerCollection indexHandlers,
        ILogger<ApiContentQueryService> logger,
        IVariationContextAccessor variationContextAccessor)
    {
        _examineManager = examineManager;
        _requestStartItemProviderAccessor = requestStartItemProviderAccessor;
        _selectorHandlers = selectorHandlers;
        _filterHandlers = filterHandlers;
        _sortHandlers = sortHandlers;
        _variationContextAccessor = variationContextAccessor;
        _logger = logger;

        // A fallback value is needed for Examine queries in case we don't have a value - we can't pass null or empty string
        // It is set to a random guid since this would be highly unlikely to yield any results
        _fallbackGuidValue = Guid.NewGuid().ToString("D");

        // build a look-up dictionary of field types by field name
        _fieldTypes = indexHandlers
            .SelectMany(handler => handler.GetFields())
            .DistinctBy(field => field.FieldName)
            .ToDictionary(field => field.FieldName, field => field.FieldType, StringComparer.InvariantCultureIgnoreCase);
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

        // Item culture must be either the requested culture or "none"
        var culture = CurrentCulture();
        queryOperation.And().GroupedOr(new[] { "culture" }, culture.ToLowerInvariant().IfNullOrWhiteSpace(_fallbackGuidValue), "none");

        // Handle Filtering
        HandleFiltering(filters, queryOperation);

        // Handle Sorting
        IOrdering sortQuery = HandleSorting(sorts, queryOperation).SelectFields(_itemIdOnlyFieldSet);

        ISearchResults? results = sortQuery
            .SelectFields(_itemIdOnlyFieldSet)
            .Execute(QueryOptions.SkipTake(skip, take));

        if (results is null)
        {
            return emptyResult;
        }

        Guid[] items = results
            .Where(r => r.Values.ContainsKey("itemId"))
            .Select(r => Guid.Parse(r.Values["itemId"]))
            .ToArray();
        return new PagedModel<Guid>(results.TotalItemCount, items);
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

    private IOrdering HandleSorting(IEnumerable<string> sorts, IBooleanOperation queryCriteria)
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

            if (_fieldTypes.TryGetValue(sort.FieldName, out FieldType fieldType) is false)
            {
                _logger.LogWarning("Sort implementation for field name {FieldName} does not match an index handler implementation, cannot resolve field type.", sort.FieldName);
                continue;
            }

            SortType sortType = fieldType switch
            {
                FieldType.Number => SortType.Int,
                FieldType.Date => SortType.Long,
                FieldType.StringRaw => SortType.String,
                FieldType.StringAnalyzed => SortType.String,
                FieldType.StringSortable => SortType.String,
                _ => throw new ArgumentOutOfRangeException(nameof(fieldType))
            };

            orderingQuery = sort.Direction switch
            {
                Direction.Ascending => queryCriteria.OrderBy(new SortableField(sort.FieldName, sortType)),
                Direction.Descending => queryCriteria.OrderByDescending(new SortableField(sort.FieldName, sortType)),
                _ => orderingQuery
            };
        }

        return orderingQuery ?? // Apply default sorting (left-aligning the content tree) if no valid sort query params
               queryCriteria
                   .OrderBy(new SortableField(PathSortIndexer.FieldName, SortType.String))
                   .OrderBy(new SortableField(SortOrderSortIndexer.FieldName, SortType.Int));
    }

    private string CurrentCulture()
        => _variationContextAccessor.VariationContext?.Culture ?? string.Empty;
}
