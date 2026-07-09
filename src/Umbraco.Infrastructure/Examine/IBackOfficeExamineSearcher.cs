using Examine;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Used to search the back office for Examine indexed entities (Documents, Media and Members)
/// </summary>
public interface IBackOfficeExamineSearcher
{
    /// <summary>
    /// Searches the back office Examine index using the specified query and parameters.
    /// </summary>
    /// <remarks>default implementation to avoid breaking changes falls back to old behaviour</remarks>
    /// <param name="query">The search query string.</param>
    /// <param name="entityType">The type of Umbraco entity to search for.</param>
    /// <param name="pageSize">The maximum number of results to return per page.</param>
    /// <param name="pageIndex">The zero-based index of the results page to return.</param>
    /// <param name="totalFound">When this method returns, contains the total number of results matching the query.</param>
    /// <param name="contentTypeAliases">An optional array of content type aliases to filter the search results; pass <c>null</c> to include all types.</param>
    /// <param name="trashed">Optional filter to include only trashed (<c>true</c>), only non-trashed (<c>false</c>), or all (<c>null</c>) items.</param>
    /// <param name="searchFrom">An optional starting point (node ID or path) for the search; pass <c>null</c> to search from the root.</param>
    /// <param name="ignoreUserStartNodes">If <c>true</c>, ignores user start nodes when searching; otherwise, respects user permissions.</param>
    /// <returns>An enumerable collection of <see cref="ISearchResult"/> objects matching the query and filters.</returns>
    IEnumerable<ISearchResult> Search(
        string query,
        UmbracoEntityTypes entityType,
        int pageSize,
        long pageIndex,
        out long totalFound,
        string[]? contentTypeAliases,
        bool? trashed,
        string? searchFrom = null,
        bool ignoreUserStartNodes = false);
}
