namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

public record IntegerExactFilter(string FieldName, int[] Values, bool Negate)
    : ExactFilter<int>(FieldName, Values, Negate)
{
}
