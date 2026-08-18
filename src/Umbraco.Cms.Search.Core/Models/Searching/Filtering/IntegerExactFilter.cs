namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

/// <summary>
/// Filters on an exact-match integer field.
/// </summary>
/// <param name="FieldName">The name of the integer field to filter on.</param>
/// <param name="Values">The integer values to match.</param>
/// <param name="Negate">If true, matches documents whose field value is none of <paramref name="Values"/>.</param>
public record IntegerExactFilter(string FieldName, int[] Values, bool Negate)
    : ExactFilter<int>(FieldName, Values, Negate)
{
}
