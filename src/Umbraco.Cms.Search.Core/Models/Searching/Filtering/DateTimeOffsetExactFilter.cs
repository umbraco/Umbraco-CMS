namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

public record DateTimeOffsetExactFilter(string FieldName, DateTimeOffset[] Values, bool Negate)
    : ExactFilter<DateTimeOffset>(FieldName, Values, Negate)
{
}
