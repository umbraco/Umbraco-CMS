using Examine;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Infrastructure.Examine;

public class NoopBackOfficeExamineSearcher : IBackOfficeExamineSearcher
{
    public IEnumerable<ISearchResult> Search(
        string query,
        UmbracoEntityTypes entityType,
        int pageSize,
        long pageIndex,
        out long totalFound,
        string? searchFrom = null,
        bool ignoreUserStartNodes = false)
    {
        totalFound = 0;
        return Enumerable.Empty<ISearchResult>();
    }
}
