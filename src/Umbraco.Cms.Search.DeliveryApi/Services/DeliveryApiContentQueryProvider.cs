using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Search.Core.Helpers;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Search.Core.Constants;

namespace Umbraco.Cms.Search.DeliveryApi.Services;

// TODO: implement IApiMediaQueryProvider when that's a thing
internal sealed class DeliveryApiContentQueryProvider : IApiContentQueryProvider
{
    private readonly ISearcher _searcher;
    private readonly IDateTimeOffsetConverter _dateTimeOffsetConverter;
    private readonly IRequestMemberAccessService _requestMemberAccessService;
    private readonly IMemberService _memberService;
    private readonly IPublishedContentTypeCache _publishedContentTypeCache;
    private readonly ILogger<DeliveryApiContentQueryProvider> _logger;
    private readonly Dictionary<string, FieldType> _fieldTypes;

    public DeliveryApiContentQueryProvider(
        ISearcher searcher,
        ContentIndexHandlerCollection contentIndexHandlerCollection,
        IRequestMemberAccessService requestMemberAccessService,
        IMemberService memberService,
        IPublishedContentTypeCache publishedContentTypeCache,
        IDateTimeOffsetConverter dateTimeOffsetConverter,
        ILogger<DeliveryApiContentQueryProvider> logger)
    {
        _searcher = searcher;
        _dateTimeOffsetConverter = dateTimeOffsetConverter;
        _logger = logger;
        _memberService = memberService;
        _publishedContentTypeCache = publishedContentTypeCache;
        _requestMemberAccessService = requestMemberAccessService;

        // build a look-up dictionary of field types by field name
        _fieldTypes = contentIndexHandlerCollection
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
        var filters = new List<Filter>();

        if (selectorOption.Values.Length > 0 && TryCreateFilter(selectorOption.FieldName, FilterOperation.Is, selectorOption.Values, out Filter? selectorOptionFilter))
        {
            filters.Add(selectorOptionFilter);
        }

        foreach (FilterOption filterOption in filterOptions)
        {
            if (TryCreateFilter(filterOption.FieldName, filterOption.Operator, filterOption.Values, out Filter? filterOptionFilter))
            {
                filters.Add(filterOptionFilter);
            }
        }

        Sorter[]? sorters = sortOptions
            .Select(sortOption => TryCreateSorter(sortOption.FieldName, sortOption.Direction, out Sorter? sorter)
                ? sorter
                : null)
            .WhereNotNull()
            .ToArray();

        sorters = sorters.Length is 0
            ? null
            : sorters;

        AccessContext? accessContext = GetAccessContextAsync().GetAwaiter().GetResult();

        var indexAlias = preview ? Constants.IndexAliases.DraftContent : Constants.IndexAliases.PublishedContent;
        SearchResult result = _searcher
            .SearchAsync(indexAlias, null, filters, null, sorters, culture, null, accessContext, skip, take)
            .GetAwaiter()
            .GetResult();

        return new PagedModel<Guid>(result.Total, result.Documents.Select(document => document.Id).ToArray());
    }

    public SelectorOption AllContentSelectorOption()
        => new() { FieldName = string.Empty, Values = [] };

    private bool TryCreateFilter(string fieldName, FilterOperation filterOperation, string[] values, [NotNullWhen(true)] out Filter? filter)
    {
        if (values.Length is 0)
        {
            filter = null;
            return false;
        }

        if (_fieldTypes.TryGetValue(fieldName, out FieldType fieldType) is false)
        {
            _logger.LogWarning(
                "Filter implementation for field name {FieldName} does not match an index handler implementation, cannot resolve field type.",
                fieldName);
            filter = null;
            return false;
        }

        fieldName = MapSystemFieldName(fieldName);
        values = MapSystemFieldValues(fieldName, values);

        switch (fieldType)
        {
            case FieldType.StringRaw:
                filter = filterOperation switch
                {
                    FilterOperation.Is or FilterOperation.IsNot => new KeywordFilter(fieldName, values, filterOperation is FilterOperation.IsNot),
                    _ => null
                };
                break;
            case FieldType.StringAnalyzed:
            case FieldType.StringSortable:
                filter = filterOperation switch
                {
                    FilterOperation.Is or FilterOperation.IsNot => new KeywordFilter(fieldName, values, filterOperation is FilterOperation.IsNot),
                    FilterOperation.Contains or FilterOperation.DoesNotContain => new TextFilter(fieldName, values, filterOperation is FilterOperation.DoesNotContain),
                    _ => null
                };
                break;
            case FieldType.Number:
                var decimalValues = values
                    .Select(v => decimal.TryParse(v, CultureInfo.InvariantCulture, out var d) ? d : decimal.MinValue)
                    .Where(d => d > decimal.MinValue)
                    .ToArray();
                if (decimalValues.Length is 0)
                {
                    _logger.LogWarning("Numeric filter for field name {FieldName} did not yield any numeric values.", fieldName);
                    filter = null;
                    return false;
                }
                filter = filterOperation switch
                {
                    FilterOperation.Is or FilterOperation.IsNot => new DecimalExactFilter(fieldName, decimalValues, filterOperation is FilterOperation.IsNot),
                    FilterOperation.LessThan => DecimalRangeFilter.Single(fieldName, null, decimalValues[0] - 0.001m, false),
                    FilterOperation.LessThanOrEqual => DecimalRangeFilter.Single(fieldName, null, decimalValues[0], false),
                    FilterOperation.GreaterThan => DecimalRangeFilter.Single(fieldName, decimalValues[0] + 0.001m, null, false),
                    FilterOperation.GreaterThanOrEqual => DecimalRangeFilter.Single(fieldName, decimalValues[0], null, false),
                    _ => null
                };
                break;
            case FieldType.Date:
                DateTimeOffset[] dateTimeOffsetValues = values
                    .Select(v => DateTime.TryParse(v, CultureInfo.InvariantCulture, out DateTime d) ? d : DateTime.MinValue)
                    .Where(d => d > DateTime.MinValue)
                    .Select(_dateTimeOffsetConverter.ToDateTimeOffset)
                    .ToArray();
                if (dateTimeOffsetValues.Length is 0)
                {
                    _logger.LogWarning("Date filter for field name {FieldName} did not yield any DateTimeOffset values.", fieldName);
                    filter = null;
                    return false;
                }
                filter = filterOperation switch
                {
                    FilterOperation.Is or FilterOperation.IsNot => new DateTimeOffsetExactFilter(fieldName, dateTimeOffsetValues, filterOperation is FilterOperation.IsNot),
                    FilterOperation.LessThan => DateTimeOffsetRangeFilter.Single(fieldName, null, dateTimeOffsetValues[0].AddMilliseconds(-1), false),
                    FilterOperation.LessThanOrEqual => DateTimeOffsetRangeFilter.Single(fieldName, null, dateTimeOffsetValues[0], false),
                    FilterOperation.GreaterThan => DateTimeOffsetRangeFilter.Single(fieldName, dateTimeOffsetValues[0].AddMilliseconds(1), null, false),
                    FilterOperation.GreaterThanOrEqual => DateTimeOffsetRangeFilter.Single(fieldName, dateTimeOffsetValues[0], null, false),
                    _ => null
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(fieldType), fieldType, null);
        }

        return filter != null;
    }

    private bool TryCreateSorter(string fieldName, Direction direction, [NotNullWhen(true)] out Sorter? sorter)
    {
        if (_fieldTypes.TryGetValue(fieldName, out FieldType fieldType) is false)
        {
            _logger.LogWarning(
                "Sorter implementation for field name {FieldName} does not match an index handler implementation, cannot resolve field type.",
                fieldName);
            sorter = null;
            return false;
        }

        fieldName = MapSystemFieldName(fieldName);

        if (fieldName is Constants.FieldNames.Level or Constants.FieldNames.SortOrder)
        {
            sorter = new IntegerSorter(fieldName, direction);
            return true;
        }

        sorter = fieldType switch
        {
            FieldType.StringRaw => new KeywordSorter(fieldName, direction),
            FieldType.StringAnalyzed or FieldType.StringSortable => new TextSorter(fieldName, direction),
            FieldType.Number => new DecimalSorter(fieldName, direction),
            FieldType.Date => new DateTimeOffsetSorter(fieldName, direction),
            _ => throw new ArgumentOutOfRangeException(nameof(fieldType), fieldType, null)
        };

        return true;
    }

    // hardcoded mapping from the old Delivery API fields to the search abstraction ones
    private static string MapSystemFieldName(string fieldName)
        => fieldName switch
        {
            // AncestorsSelectorIndexer:
            "itemId" => Constants.FieldNames.Id,
            // ChildrenSelectorIndexer:
            "parentId" => Constants.FieldNames.ParentId,
            // DescendantsSelectorIndexer:
            // TODO: this is somewhat wrong... PathIds equals ancestors-or-self, but the Delivery API queries for ancestors only
            "ancestorIds" => Constants.FieldNames.PathIds,
            // ContentTypeFilterIndexer:
            "contentType" => Constants.FieldNames.ContentTypeId,
            // NameFilterIndexer or NameSortIndexer:
            "name" or "sortName" => Constants.FieldNames.Name,
            // CreateDateSortIndexer
            "createDate" => Constants.FieldNames.CreateDate,
            // UpdateDateSortIndexer
            "updateDate" => Constants.FieldNames.UpdateDate,
            // LevelSortIndexer
            "level" => Constants.FieldNames.Level,
            // SortOrderSortIndexer
            "sortOrder" => Constants.FieldNames.SortOrder,
            _ => fieldName
        };

    private async Task<AccessContext?> GetAccessContextAsync()
    {
        ProtectedAccess memberAccess = await _requestMemberAccessService.MemberAccessAsync();
        if (memberAccess.MemberKey.HasValue is false)
        {
            return null;
        }

        Guid[]? memberGroupKeys = memberAccess.MemberRoles is { Length: > 0 }
            ? _memberService
                .GetAllRoles()
                .Where(group => memberAccess.MemberRoles!.InvariantContains(group.Name ?? string.Empty))
                .Select(group => group.Key)
                .ToArray()
            : null;

        return new AccessContext(memberAccess.MemberKey.Value, memberGroupKeys);
    }

    private string[] MapSystemFieldValues(string fieldName, string[] values)
    {
        if (fieldName is not Constants.FieldNames.ContentTypeId)
        {
            return values;
        }

        if (values.Length is 0)
        {
            return values;
        }

        if (Guid.TryParse(values[0], out _))
        {
            // assume it's an array of keyword representations of content type keys
            return values;
        }

        // assume it's an array of content type aliases
        return values.Select(alias =>
            {
                // the published content type cache throws an exception if no content type is found by alias
                try
                {
                    return _publishedContentTypeCache.Get(PublishedItemType.Content, alias).Key.AsKeyword();
                }
                catch
                {
                    return null;
                }
            })
            .WhereNotNull()
            .ToArray();
    }
}
