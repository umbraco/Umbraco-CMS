namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

public record DateTimeOffsetRangeFilter(string FieldName, DateTimeOffsetRangeFilterRange[] Ranges, bool Negate)
    : RangeFilter<DateTimeOffsetRangeFilterRange>(FieldName, Ranges, Negate)
{
    public static DateTimeOffsetRangeFilter Single(string fieldName, DateTimeOffset? minimumValue, DateTimeOffset? maximumValue, bool negate)
        => new (fieldName, [new DateTimeOffsetRangeFilterRange(minimumValue, maximumValue)], negate);
}
