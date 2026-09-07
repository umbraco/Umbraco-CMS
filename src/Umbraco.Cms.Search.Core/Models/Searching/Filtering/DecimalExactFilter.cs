namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

/// <summary>
/// Filters on an exact-match decimal field.
/// </summary>
/// <param name="FieldName">The name of the decimal field to filter on.</param>
/// <param name="Values">The decimal values to match.</param>
/// <param name="Negate">If true, matches documents whose field value is none of <paramref name="Values"/>.</param>
public record DecimalExactFilter(string FieldName, decimal[] Values, bool Negate)
    : ExactFilter<decimal>(FieldName, Values, Negate)
{
}
