using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Faceting;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;

namespace Umbraco.Cms.Search.Core.Services;

/// <summary>
/// Executes searches (filtering, faceting, sorting, pagination) against a search index. Implemented by search providers (e.g. Examine).
/// </summary>
public interface ISearcher
{
    /// <summary>
    /// Executes a search against the index.
    /// </summary>
    /// <param name="indexAlias">The alias of the index to search.</param>
    /// <param name="query">The full-text search query, matched against Text fields.</param>
    /// <param name="filters">Filters to apply. Filters are ANDed together; values within a single filter are ORed.</param>
    /// <param name="facets">Facets to compute alongside the search results.</param>
    /// <param name="sorters">Sorters to apply, in priority order (the first sorter is primary).</param>
    /// <param name="culture">The culture to search in. Invariant content is always included.</param>
    /// <param name="segment">The segment to search in.</param>
    /// <param name="accessContext">The member/group context to use for including protected content, if any.</param>
    /// <param name="skip">The number of results to skip, for pagination.</param>
    /// <param name="take">The maximum number of results to return.</param>
    /// <param name="maxSuggestions">The maximum number of query suggestions to return, if supported.</param>
    /// <returns>The matching documents, total count, and any computed facet results.</returns>
    Task<SearchResult> SearchAsync(
        string indexAlias,
        string? query = null,
        IEnumerable<Filter>? filters = null,
        IEnumerable<Facet>? facets = null,
        IEnumerable<Sorter>? sorters = null,
        string? culture = null,
        string? segment = null,
        AccessContext? accessContext = null,
        int skip = 0,
        int take = 10,
        int maxSuggestions = 0);
}
