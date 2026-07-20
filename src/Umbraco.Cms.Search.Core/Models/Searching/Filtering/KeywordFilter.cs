namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

public record KeywordFilter(string FieldName, string[] Values, bool Negate)
    : ExactFilter<string>(FieldName, Values, Negate)
{
}
