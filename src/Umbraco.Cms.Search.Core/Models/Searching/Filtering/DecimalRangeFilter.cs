namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

/// <summary>
/// Filters on a decimal field, matching documents whose value falls within one or more ranges.
/// </summary>
/// <param name="FieldName">The name of the decimal field to filter on.</param>
/// <param name="Ranges">The ranges to match. A document matches if its field value falls within any one of these.</param>
/// <param name="Negate">If true, matches documents whose field value falls outside all of <paramref name="Ranges"/>.</param>
public record DecimalRangeFilter(string FieldName, DecimalRangeFilterRange[] Ranges, bool Negate)
    : RangeFilter<DecimalRangeFilterRange>(FieldName, Ranges, Negate)
{
    /// <summary>
    /// Creates a filter for a single range.
    /// </summary>
    /// <param name="fieldName">The name of the decimal field to filter on.</param>
    /// <param name="minimumValue">The inclusive lower bound, or null for no lower bound.</param>
    /// <param name="maximumValue">The exclusive upper bound, or null for no upper bound.</param>
    /// <param name="negate">If true, matches documents whose field value falls outside the range.</param>
    /// <returns>The resulting filter.</returns>
    public static DecimalRangeFilter Single(string fieldName, decimal? minimumValue, decimal? maximumValue, bool negate)
        => new (fieldName, [new DecimalRangeFilterRange(minimumValue, maximumValue)], negate);
}
