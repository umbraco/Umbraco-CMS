using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Search;

namespace Umbraco.Search.SpecialisedSearchers;

public class NoopBackOfficeExamineSearcher : IBackOfficeExamineSearcher
{
    public IEnumerable<IUmbracoSearchResult> Search(string query, UmbracoEntityTypes entityType, int pageSize,
        long pageIndex, out long totalFound,
        string? searchFrom = null, bool ignoreUserStartNodes = false)
    {
        totalFound = 0;
        return new List<IUmbracoSearchResult>(0);
    }
}
