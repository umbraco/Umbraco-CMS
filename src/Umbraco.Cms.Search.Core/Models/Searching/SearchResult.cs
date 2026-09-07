using Umbraco.Cms.Search.Core.Models.Searching.Faceting;

namespace Umbraco.Cms.Search.Core.Models.Searching;

/// <summary>
/// Represents a page of search results, as returned by <see cref="Umbraco.Cms.Search.Core.Services.ISearcher.SearchAsync"/>.
/// </summary>
/// <param name="Total">The total number of matching documents, regardless of paging.</param>
/// <param name="Documents">The matched documents for the requested page.</param>
/// <param name="Facets">The facet results for the requested facets, if any.</param>
/// <param name="Suggestions">Query suggestions (e.g. "did you mean"), if requested and available.</param>
public record SearchResult(long Total, IEnumerable<Document> Documents, IEnumerable<FacetResult> Facets, IEnumerable<string>? Suggestions = null)
{
}
