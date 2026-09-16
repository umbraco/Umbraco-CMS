namespace Umbraco.Cms.Search.Core.Models.Searching.Faceting;

/// <summary>
/// The facet result for a single requested field: its computed bucket values.
/// </summary>
/// <param name="FieldName">The name of the index field the facet was computed on.</param>
/// <param name="Values">The resulting bucket values.</param>
public record FacetResult(string FieldName, IEnumerable<FacetValue> Values)
{
}
