namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

public record FacetResult(string FieldName, IEnumerable<FacetValue> Values)
{
}
