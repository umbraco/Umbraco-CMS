using Examine;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
/// Represents a no-operation (noop) implementation of the back office Examine searcher, which performs no search actions.
/// This can be used as a placeholder or default when search functionality is not required.
/// </summary>
public class NoopBackOfficeExamineSearcher : IBackOfficeExamineSearcher
{
    /// <summary>
    /// Simulates a search against the back office examine index, but always returns no results.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="entityType">The type of Umbraco entity to search for.</param>
    /// <param name="pageSize">The number of results to return per page.</param>
    /// <param name="pageIndex">The index of the page of results to return.</param>
    /// <param name="totalFound">Outputs the total number of results found (always zero).</param>
    /// <param name="contentTypeAliases">Optional array of content type aliases to filter the search.</param>
    /// <param name="trashed">Optional filter to include trashed items.</param>
    /// <param name="searchFrom">Optional starting point for the search.</param>
    /// <param name="ignoreUserStartNodes">Whether to ignore user start nodes in the search.</param>
    /// <returns>An empty enumerable, as this implementation does not perform any search.</returns>
    public IEnumerable<ISearchResult> Search(
        string query,
        UmbracoEntityTypes entityType,
        int pageSize,
        long pageIndex,
        out long totalFound,
        string[]? contentTypeAliases,
        bool? trashed,
        string? searchFrom = null,
        bool ignoreUserStartNodes = false)
    {
        totalFound = 0;
        return Enumerable.Empty<ISearchResult>();
    }
}
