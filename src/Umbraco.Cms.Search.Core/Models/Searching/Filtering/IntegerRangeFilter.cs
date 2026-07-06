namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

public record IntegerRangeFilter(string FieldName, IntegerRangeFilterRange[] Ranges, bool Negate)
    : RangeFilter<IntegerRangeFilterRange>(FieldName, Ranges, Negate)
{
    public static IntegerRangeFilter Single(string fieldName, int? minimumValue, int? maximumValue, bool negate)
        => new (fieldName, [new IntegerRangeFilterRange(minimumValue, maximumValue)], negate);
}
