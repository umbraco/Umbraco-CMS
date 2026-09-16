namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

/// <summary>
/// Filters on an exact-match date/time field.
/// </summary>
/// <param name="FieldName">The name of the date/time field to filter on.</param>
/// <param name="Values">The date/time values to match.</param>
/// <param name="Negate">If true, matches documents whose field value is none of <paramref name="Values"/>.</param>
public record DateTimeOffsetExactFilter(string FieldName, DateTimeOffset[] Values, bool Negate)
    : ExactFilter<DateTimeOffset>(FieldName, Values, Negate)
{
}
