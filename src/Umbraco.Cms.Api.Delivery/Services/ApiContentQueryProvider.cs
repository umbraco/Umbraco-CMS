using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.Search;
using Umbraco.Extensions;
using Umbraco.Search;
using Umbraco.Search.Models;

namespace Umbraco.Cms.Api.Delivery.Services;

/// <summary>
/// This is the Examine implementation of content querying for the Delivery API.
/// </summary>
internal sealed class ApiContentQueryProvider : IApiContentQueryProvider
{
    private const string ItemIdFieldName = "itemId";
    private readonly ISearchProvider _examineManager;
    private readonly ILogger<ApiContentQueryProvider> _logger;
    private readonly string _fallbackGuidValue;
    private readonly Dictionary<string, FieldType> _fieldTypes;

    public ApiContentQueryProvider(
        ISearchProvider examineManager,
        ContentIndexHandlerCollection indexHandlers,
        ILogger<ApiContentQueryProvider> logger)
    {
        _examineManager = examineManager;
        _logger = logger;

        // A fallback value is needed for Examine queries in case we don't have a value - we can't pass null or empty string
        // It is set to a random guid since this would be highly unlikely to yield any results
        _fallbackGuidValue = Guid.NewGuid().ToString("D");

        // build a look-up dictionary of field types by field name
        _fieldTypes = indexHandlers
            .SelectMany(handler => handler.GetFields())
            .DistinctBy(field => field.FieldName)
            .ToDictionary(field => field.FieldName, field => field.FieldType,
                StringComparer.InvariantCultureIgnoreCase);
    }

    public PagedModel<Guid> ExecuteQuery(SelectorOption selectorOption, IList<FilterOption> filterOptions,
        IList<SortOption> sortOptions, string culture, int skip, int take)
    {
        var searcher = _examineManager.GetSearcher(Constants.UmbracoIndexes.DeliveryApiContentIndexName);
        if (searcher == null)
        {
            _logger.LogError("Could not find the searcher for {IndexName} when attempting to execute a query.",
                Constants.UmbracoIndexes.DeliveryApiContentIndexName);
            return new PagedModel<Guid>();
        }


        ISearchRequest queryOperation = BuildSelectorOperation(selectorOption, searcher, culture);

        ApplyFiltering(filterOptions, queryOperation);
        ApplySorting(sortOptions, queryOperation);

//todo: figure out pagination of this query
        IUmbracoSearchResults? results =
            searcher.Search(queryOperation);

        if (results is null)
        {
            // The query yield no results
            return new PagedModel<Guid>();
        }

        Guid[] items = results
            .Where(r => r.Values.ContainsKey(ItemIdFieldName))
            .Select(r => r.Values[ItemIdFieldName][0]?.ToString() ?? string.Empty)
            .Where(r => !string.IsNullOrWhiteSpace(r)).Select(Guid.Parse)
            .ToArray();

        return new PagedModel<Guid>(results.TotalItemCount, items);
    }

    public SelectorOption AllContentSelectorOption() => new()
    {
        FieldName = UmbracoSearchFieldNames.CategoryFieldName, Values = new[] { "content" }
    };

    private ISearchRequest BuildSelectorOperation(SelectorOption selectorOption, IUmbracoSearcher searcher,
        string culture)
    {
        ISearchRequest searchRequest = searcher.CreateSearchRequest();
        searchRequest.FiltersLogicOperator = LogicOperator.And;
        searchRequest.CreateFilter(selectorOption.FieldName, selectorOption.Values.ToList(), LogicOperator.OR);
        searchRequest.CreateFilter(UmbracoSearchFieldNames.DeliveryApiContentIndex.Culture,
            new List<string>() { culture.ToLowerInvariant().IfNullOrWhiteSpace(_fallbackGuidValue), "none" },
            LogicOperator.OR);



        return searchRequest;
    }

    private void ApplyFiltering(IList<FilterOption> filterOptions, ISearchRequest queryOperation)
    {
        void HandleExact(ISearchFilter query, string fieldName, string[] values, LogicOperator logicOperator)
        {
            query.CreateSubFilter(fieldName, values.ToList(), logicOperator);
        }
var defaultFilter = new DefaultSearchFilter("filters", new List<string>(), LogicOperator.And, new List<ISearchFilter>());
        foreach (FilterOption filterOption in filterOptions)
        {
            var values = filterOption.Values.Any()
                ? filterOption.Values
                : new[] { _fallbackGuidValue };

            switch (filterOption.Operator)
            {
                case FilterOperation.Is:
                    // TODO: test this for explicit word matching
                    HandleExact(defaultFilter, filterOption.FieldName, values, LogicOperator.And);
                    break;
                case FilterOperation.IsNot:
                    // TODO: test this for explicit word matching
                    HandleExact(defaultFilter, filterOption.FieldName, values, LogicOperator.Not);
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

    private void ApplySorting(IList<SortOption> sortOptions, ISearchRequest ordering)
    {
        foreach (SortOption sort in sortOptions)
        {
            if (_fieldTypes.TryGetValue(sort.FieldName, out FieldType fieldType) is false)
            {
                _logger.LogWarning(
                    "Sort implementation for field name {FieldName} does not match an index handler implementation, cannot resolve field type.",
                    sort.FieldName);
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
            ordering.SortBy(sort.FieldName, sortType);
        }
    }
}
