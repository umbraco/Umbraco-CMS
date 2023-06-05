using Examine;
using Examine.Search;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Api.Delivery.Services;

/// <summary>
/// This is the Examine implementation of content querying for the Delivery API.
/// </summary>
internal sealed class ApiContentQueryProvider : IApiContentQueryProvider
{
    private const string ItemIdFieldName = "itemId";
    private readonly IExamineManager _examineManager;
    private readonly ILogger<ApiContentQueryProvider> _logger;
    private readonly string _fallbackGuidValue;
    private readonly Dictionary<string, FieldType> _fieldTypes;

    public ApiContentQueryProvider(
        IExamineManager examineManager,
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
            .ToDictionary(field => field.FieldName, field => field.FieldType, StringComparer.InvariantCultureIgnoreCase);
    }

    public PagedModel<Guid> ExecuteQuery(SelectorOption selectorOption, IList<FilterOption> filterOptions, IList<SortOption> sortOptions, string culture, int skip, int take)
    {
        if (!_examineManager.TryGetIndex(Constants.UmbracoIndexes.DeliveryApiContentIndexName, out IIndex? index))
        {
            _logger.LogError("Could not find the index {IndexName} when attempting to execute a query.", Constants.UmbracoIndexes.DeliveryApiContentIndexName);
            return new PagedModel<Guid>();
        }

        IBooleanOperation queryOperation = BuildSelectorOperation(selectorOption, index, culture);

        ApplyFiltering(filterOptions, queryOperation);
        ApplySorting(sortOptions, queryOperation);

        ISearchResults? results = queryOperation
            .SelectField(ItemIdFieldName)
            .Execute(QueryOptions.SkipTake(skip, take));

        if (results is null)
        {
            // The query yield no results
            return new PagedModel<Guid>();
        }

        Guid[] items = results
            .Where(r => r.Values.ContainsKey(ItemIdFieldName))
            .Select(r => Guid.Parse(r.Values[ItemIdFieldName]))
            .ToArray();

        return new PagedModel<Guid>(results.TotalItemCount, items);
    }

    public SelectorOption AllContentSelectorOption() => new()
    {
        FieldName = UmbracoExamineFieldNames.CategoryFieldName, Values = new[] { "content" }
    };

    private IBooleanOperation BuildSelectorOperation(SelectorOption selectorOption, IIndex index, string culture)
    {
        IQuery query = index.Searcher.CreateQuery();

        IBooleanOperation selectorOperation = selectorOption.Values.Length == 1
            ? query.Field(selectorOption.FieldName, selectorOption.Values.First())
            : query.GroupedOr(new[] { selectorOption.FieldName }, selectorOption.Values);

        // Item culture must be either the requested culture or "none"
        selectorOperation.And().GroupedOr(new[] { UmbracoExamineFieldNames.DeliveryApiContentIndex.Culture }, culture.ToLowerInvariant().IfNullOrWhiteSpace(_fallbackGuidValue), "none");

        return selectorOperation;
    }

    private void ApplyFiltering(IList<FilterOption> filterOptions, IBooleanOperation queryOperation)
    {
        void HandleExact(IQuery query, string fieldName, string[] values)
        {
            if (values.Length == 1)
            {
                query.Field(fieldName, values[0]);
            }
            else
            {
                query.GroupedOr(new[] { fieldName }, values);
            }
        }

        foreach (FilterOption filterOption in filterOptions)
        {
            var values = filterOption.Values.Any()
                ? filterOption.Values
                : new[] { _fallbackGuidValue };

            switch (filterOption.Operator)
            {
                case FilterOperation.Is:
                    // TODO: test this for explicit word matching
                    HandleExact(queryOperation.And(), filterOption.FieldName, values);
                    break;
                case FilterOperation.IsNot:
                    // TODO: test this for explicit word matching
                    HandleExact(queryOperation.Not(), filterOption.FieldName, values);
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

    private void ApplySorting(IList<SortOption> sortOptions, IOrdering ordering)
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

            ordering = sort.Direction switch
            {
                Direction.Ascending => ordering.OrderBy(new SortableField(sort.FieldName, sortType)),
                Direction.Descending => ordering.OrderByDescending(new SortableField(sort.FieldName, sortType)),
                _ => ordering
            };
        }
    }
}
