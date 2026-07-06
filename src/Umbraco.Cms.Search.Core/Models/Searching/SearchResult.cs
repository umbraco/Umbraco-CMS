using Umbraco.Cms.Search.Core.Models.Searching.Faceting;

namespace Umbraco.Cms.Search.Core.Models.Searching;

public record SearchResult(long Total, IEnumerable<Document> Documents, IEnumerable<FacetResult> Facets, IEnumerable<string>? Suggestions = null)
{
}
