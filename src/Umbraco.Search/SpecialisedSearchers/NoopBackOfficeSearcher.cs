using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Search.Models;

namespace Umbraco.Search.SpecialisedSearchers;

public class NoopBackOfficeSearcher : IBackOfficeSearcher
{
    public IEnumerable<IUmbracoSearchResult> Search(
        IBackofficeSearchRequest request,
        out long totalFound)
    {
        totalFound = 0;
        return new List<IUmbracoSearchResult>(0);
    }
}
