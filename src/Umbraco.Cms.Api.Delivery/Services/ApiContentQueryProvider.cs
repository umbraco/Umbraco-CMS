using Examine;
using Examine.Lucene.Providers;
using Examine.Lucene.Search;
using Examine.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Services;

/// <summary>
/// This is the Examine implementation of content querying for the Delivery API.
/// </summary>
internal sealed class ApiContentQueryProvider : IApiContentQueryProvider
{
    private const string ItemIdFieldName = "itemId";
    private readonly IExamineManager _examineManager;
    private readonly DeliveryApiSettings _deliveryApiSettings;
    private readonly ILogger<ApiContentQueryProvider> _logger;
    private readonly string _fallbackGuidValue;
    private readonly Dictionary<string, FieldType> _fieldTypes;

    public ApiContentQueryProvider(
        IExamineManager examineManager,
        ContentIndexHandlerCollection indexHandlers,
        IOptions<DeliveryApiSettings> deliveryApiSettings,
        ILogger<ApiContentQueryProvider> logger)
    {
        _examineManager = examineManager;
        _deliveryApiSettings = deliveryApiSettings.Value;
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

    [Obsolete($"Use the {nameof(ExecuteQuery)} method that accepts {nameof(ProtectedAccess)}. Will be removed in V14.")]
    public PagedModel<Guid> ExecuteQuery(
        SelectorOption selectorOption,
        IList<FilterOption> filterOptions,
        IList<SortOption> sortOptions,
        string culture,
        bool preview,
        int skip,
        int take)
        => ExecuteQuery(selectorOption, filterOptions, sortOptions, culture, ProtectedAccess.None, preview, skip, take);

    /// <inheritdoc/>
    public PagedModel<Guid> ExecuteQuery(
        SelectorOption selectorOption,
        IList<FilterOption> filterOptions,
        IList<SortOption> sortOptions,
        string culture,
        ProtectedAccess protectedAccess,
        bool preview,
        int skip,
        int take)
    {
        if (!_examineManager.TryGetIndex(Constants.UmbracoIndexes.DeliveryApiContentIndexName, out IIndex? index))
        {
            _logger.LogError("Could not find the index {IndexName} when attempting to execute a query.", Constants.UmbracoIndexes.DeliveryApiContentIndexName);
            return new PagedModel<Guid>();
        }

        IBooleanOperation queryOperation = BuildSelectorOperation(selectorOption, index, culture, protectedAccess, preview);

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

    private IBooleanOperation BuildSelectorOperation(SelectorOption selectorOption, IIndex index, string culture, ProtectedAccess protectedAccess, bool preview)
    {
        // Needed for enabling leading wildcards searches
        BaseLuceneSearcher searcher = index.Searcher as BaseLuceneSearcher ?? throw new InvalidOperationException($"Index searcher must be of type {nameof(BaseLuceneSearcher)}.");

        IQuery query = searcher.CreateQuery(
            IndexTypes.Content,
            BooleanOperation.And,
            searcher.LuceneAnalyzer,
            new LuceneSearchOptions { AllowLeadingWildcard = true });

        IBooleanOperation selectorOperation = selectorOption.Values.Length == 1
            ? query.Field(selectorOption.FieldName, selectorOption.Values.First())
            : query.GroupedOr(new[] { selectorOption.FieldName }, selectorOption.Values);

        AddCultureQuery(culture, selectorOperation);

        if (_deliveryApiSettings.MemberAuthorizationIsEnabled())
        {
            AddProtectedAccessQuery(protectedAccess, selectorOperation);
        }

        // when not fetching for preview, make sure the "published" field is "y"
        if (preview is false)
        {
            selectorOperation.And().Field(UmbracoExamineFieldNames.DeliveryApiContentIndex.Published, "y");
        }

        return selectorOperation;
    }

    private void AddCultureQuery(string culture, IBooleanOperation selectorOperation) =>
        selectorOperation
            .And()
            .GroupedOr(
                // Item culture must be either the requested culture or "none"
                new[] { UmbracoExamineFieldNames.DeliveryApiContentIndex.Culture },
                culture.ToLowerInvariant().IfNullOrWhiteSpace(_fallbackGuidValue),
                "none");

    private void AddProtectedAccessQuery(ProtectedAccess protectedAccess, IBooleanOperation selectorOperation)
    {
        var protectedAccessValues = new List<string>();
        if (protectedAccess.MemberKey is not null)
        {
            protectedAccessValues.Add($"u:{protectedAccess.MemberKey}");
        }

        if (protectedAccess.MemberRoles?.Any() is true)
        {
            protectedAccessValues.AddRange(protectedAccess.MemberRoles.Select(r => $"r:{r}"));
        }

        if (protectedAccessValues.Any())
        {
            selectorOperation.And(
                inner => inner
                    .Field(UmbracoExamineFieldNames.DeliveryApiContentIndex.Protected, "n")
                    .Or(i2 => i2
                        .GroupedOr(
                            new[] { UmbracoExamineFieldNames.DeliveryApiContentIndex.ProtectedAccess },
                            protectedAccessValues.ToArray())),
                BooleanOperation.Or);
        }
        else
        {
            selectorOperation.And().Field(UmbracoExamineFieldNames.DeliveryApiContentIndex.Protected, "n");
        }
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

        void HandleContains(IQuery query, string fieldName, string[] values)
        {
            if (values.Length == 1)
            {
                // The trailing wildcard is added automatically
                query.Field(fieldName, (IExamineValue)new ExamineValue(Examineness.ComplexWildcard, $"*{values[0]}"));
            }
            else
            {
                // The trailing wildcard is added automatically
                IExamineValue[] examineValues = values
                    .Select(value => (IExamineValue)new ExamineValue(Examineness.ComplexWildcard, $"*{value}"))
                    .ToArray();
                query.GroupedOr(new[] { fieldName }, examineValues);
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
                    HandleExact(queryOperation.And(), filterOption.FieldName, values);
                    break;
                case FilterOperation.IsNot:
                    HandleExact(queryOperation.Not(), filterOption.FieldName, values);
                    break;
                case FilterOperation.Contains:
                    HandleContains(queryOperation.And(), filterOption.FieldName, values);
                    break;
                case FilterOperation.DoesNotContain:
                    HandleContains(queryOperation.Not(), filterOption.FieldName, values);
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
