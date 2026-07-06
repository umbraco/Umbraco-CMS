namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

public record DecimalRangeFilter(string FieldName, DecimalRangeFilterRange[] Ranges, bool Negate)
    : RangeFilter<DecimalRangeFilterRange>(FieldName, Ranges, Negate)
{
    public static DecimalRangeFilter Single(string fieldName, decimal? minimumValue, decimal? maximumValue, bool negate)
        => new (fieldName, [new DecimalRangeFilterRange(minimumValue, maximumValue)], negate);
}
