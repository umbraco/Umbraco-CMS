using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Search;

namespace Umbraco.Search.SpecialisedSearchers;

/// <summary>
///     Used to search the back office for Examine indexed entities (Documents, Media and Members)
/// </summary>
public interface IBackOfficeExamineSearcher
{
    IEnumerable<IUmbracoSearchResult> Search(
        string query,
        UmbracoEntityTypes entityType,
        int pageSize,
        long pageIndex,
        out long totalFound,
        string? searchFrom = null,
        bool ignoreUserStartNodes = false);
}
