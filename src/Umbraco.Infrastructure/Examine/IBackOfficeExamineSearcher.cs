using Examine;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Used to search the back office for Examine indexed entities (Documents, Media and Members)
/// </summary>
public interface IBackOfficeExamineSearcher
{
    [Obsolete("Please use the method that accepts all parameters. Will be removed in V17.")]
    IEnumerable<ISearchResult> Search(
        string query,
        UmbracoEntityTypes entityType,
        int pageSize,
        long pageIndex,
        out long totalFound,
        string? searchFrom = null,
        bool ignoreUserStartNodes = false);

    [Obsolete("Please use the method that accepts all parameters. Will be removed in V17.")]
    IEnumerable<ISearchResult> Search(
        string query,
        UmbracoEntityTypes entityType,
        int pageSize,
        long pageIndex,
        out long totalFound,
        string[]? contentTypeAliases,
        string? searchFrom = null,
        bool ignoreUserStartNodes = false)
        => Search(query, entityType, pageSize, pageIndex, out totalFound, searchFrom, ignoreUserStartNodes);

    // default implementation to avoid breaking changes falls back to old behaviour
    IEnumerable<ISearchResult> Search(
        string query,
        UmbracoEntityTypes entityType,
        int pageSize,
        long pageIndex,
        out long totalFound,
        string[]? contentTypeAliases,
        bool? trashed,
        string? searchFrom = null,
        bool ignoreUserStartNodes = false)
        => Search(query, entityType, pageSize, pageIndex, out totalFound, null, searchFrom, ignoreUserStartNodes);
}
