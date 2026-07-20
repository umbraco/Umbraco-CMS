namespace Umbraco.Cms.Search.Core.Models.Searching.Filtering;

public record TextFilter(string FieldName, string[] Values, bool Negate)
    : ContainsFilter<string>(FieldName, Values, Negate)
{
}
