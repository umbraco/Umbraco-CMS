using Umbraco.Cms.Api.Management.ViewModels.Search.Facets;
using Umbraco.Cms.Api.Management.ViewModels.Search.Filters;
using Umbraco.Cms.Api.Management.ViewModels.Search.Sorters;
using Umbraco.Cms.Core.Search.Querying.Faceting;
using Umbraco.Cms.Core.Search.Querying.Filtering;
using Umbraco.Cms.Core.Search.Querying.Sorting;

namespace Umbraco.Cms.Api.Management.Factories;

/// <inheritdoc />
public class SearchPresentationFactory : ISearchPresentationFactory
{
    /// <inheritdoc />
    public Filter CreateFilter(IFilterRequestModel filter)
        => filter switch
        {
            KeywordFilterRequestModel keyword => new KeywordFilter(keyword.FieldName, keyword.Values, keyword.Negate),
            TextFilterRequestModel text => new TextFilter(text.FieldName, text.Values, text.Negate),
            IntegerExactFilterRequestModel integerExact => new IntegerExactFilter(integerExact.FieldName, integerExact.Values, integerExact.Negate),
            DecimalExactFilterRequestModel decimalExact => new DecimalExactFilter(decimalExact.FieldName, decimalExact.Values, decimalExact.Negate),
            DateTimeOffsetExactFilterRequestModel dateTimeOffsetExact => new DateTimeOffsetExactFilter(dateTimeOffsetExact.FieldName, dateTimeOffsetExact.Values, dateTimeOffsetExact.Negate),
            IntegerRangeFilterRequestModel integerRange => new IntegerRangeFilter(
                integerRange.FieldName,
                integerRange.Ranges.Select(range => new IntegerRangeFilterRange(range.MinValue, range.MaxValue)).ToArray(),
                integerRange.Negate),
            DecimalRangeFilterRequestModel decimalRange => new DecimalRangeFilter(
                decimalRange.FieldName,
                decimalRange.Ranges.Select(range => new DecimalRangeFilterRange(range.MinValue, range.MaxValue)).ToArray(),
                decimalRange.Negate),
            DateTimeOffsetRangeFilterRequestModel dateTimeOffsetRange => new DateTimeOffsetRangeFilter(
                dateTimeOffsetRange.FieldName,
                dateTimeOffsetRange.Ranges.Select(range => new DateTimeOffsetRangeFilterRange(range.MinValue, range.MaxValue)).ToArray(),
                dateTimeOffsetRange.Negate),
            _ => throw new NotSupportedException($"Unsupported filter request model type: {filter.GetType().Name}."),
        };

    /// <inheritdoc />
    public Facet CreateFacet(IFacetRequestModel facet)
        => facet switch
        {
            KeywordFacetRequestModel keyword => new KeywordFacet(keyword.FieldName),
            IntegerExactFacetRequestModel integerExact => new IntegerExactFacet(integerExact.FieldName),
            DecimalExactFacetRequestModel decimalExact => new DecimalExactFacet(decimalExact.FieldName),
            DateTimeOffsetExactFacetRequestModel dateTimeOffsetExact => new DateTimeOffsetExactFacet(dateTimeOffsetExact.FieldName),
            IntegerRangeFacetRequestModel integerRange => new IntegerRangeFacet(
                integerRange.FieldName,
                integerRange.Ranges.Select(range => new IntegerRangeFacetRange(range.Key, range.MinValue, range.MaxValue)).ToArray()),
            DecimalRangeFacetRequestModel decimalRange => new DecimalRangeFacet(
                decimalRange.FieldName,
                decimalRange.Ranges.Select(range => new DecimalRangeFacetRange(range.Key, range.MinValue, range.MaxValue)).ToArray()),
            DateTimeOffsetRangeFacetRequestModel dateTimeOffsetRange => new DateTimeOffsetRangeFacet(
                dateTimeOffsetRange.FieldName,
                dateTimeOffsetRange.Ranges.Select(range => new DateTimeOffsetRangeFacetRange(range.Key, range.MinValue, range.MaxValue)).ToArray()),
            _ => throw new NotSupportedException($"Unsupported facet request model type: {facet.GetType().Name}."),
        };

    /// <inheritdoc />
    public Sorter CreateSorter(ISorterRequestModel sorter)
        => sorter switch
        {
            ScoreSorterRequestModel score => new ScoreSorter(score.Direction),
            KeywordSorterRequestModel keyword => new KeywordSorter(keyword.FieldName, keyword.Direction),
            TextSorterRequestModel text => new TextSorter(text.FieldName, text.Direction),
            IntegerSorterRequestModel integer => new IntegerSorter(integer.FieldName, integer.Direction),
            DecimalSorterRequestModel @decimal => new DecimalSorter(@decimal.FieldName, @decimal.Direction),
            DateTimeOffsetSorterRequestModel dateTimeOffset => new DateTimeOffsetSorter(dateTimeOffset.FieldName, dateTimeOffset.Direction),
            _ => throw new NotSupportedException($"Unsupported sorter request model type: {sorter.GetType().Name}."),
        };

    /// <inheritdoc />
    public IFacetValueResponseModel CreateFacetValueResponseModel(FacetValue facetValue)
        => facetValue switch
        {
            KeywordFacetValue keyword => new KeywordFacetValueResponseModel { Key = keyword.Key, Count = keyword.Count },
            IntegerExactFacetValue integerExact => new IntegerExactFacetValueResponseModel { Key = integerExact.Key, Count = integerExact.Count },
            DecimalExactFacetValue decimalExact => new DecimalExactFacetValueResponseModel { Key = decimalExact.Key, Count = decimalExact.Count },
            DateTimeOffsetExactFacetValue dateTimeOffsetExact => new DateTimeOffsetExactFacetValueResponseModel { Key = dateTimeOffsetExact.Key, Count = dateTimeOffsetExact.Count },
            IntegerRangeFacetValue integerRange => new IntegerRangeFacetValueResponseModel { Key = integerRange.Key, Min = integerRange.Min, Max = integerRange.Max, Count = integerRange.Count },
            DecimalRangeFacetValue decimalRange => new DecimalRangeFacetValueResponseModel { Key = decimalRange.Key, Min = decimalRange.Min, Max = decimalRange.Max, Count = decimalRange.Count },
            DateTimeOffsetRangeFacetValue dateTimeOffsetRange => new DateTimeOffsetRangeFacetValueResponseModel { Key = dateTimeOffsetRange.Key, Min = dateTimeOffsetRange.Min, Max = dateTimeOffsetRange.Max, Count = dateTimeOffsetRange.Count },
            _ => throw new NotSupportedException($"Unsupported facet value type: {facetValue.GetType().Name}."),
        };
}
