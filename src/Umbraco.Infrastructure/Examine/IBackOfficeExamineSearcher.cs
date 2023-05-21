using Examine;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Used to search the back office for Examine indexed entities (Documents, Media and Members)
/// </summary>
public interface IBackOfficeExamineSearcher
{
    IEnumerable<ISearchResult> Search(
        string query,
        UmbracoEntityTypes entityType,
        int pageSize,
        long pageIndex,
        out long totalFound,
        string? searchFrom = null,
        bool ignoreUserStartNodes = false);
}
