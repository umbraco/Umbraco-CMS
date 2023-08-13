using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Search.Models;

namespace Umbraco.Search.SpecialisedSearchers;

/// <summary>
///     Used to search the back office for Examine indexed entities (Documents, Media and Members)
/// </summary>
public interface IBackOfficeSearcher
{
    IEnumerable<IUmbracoSearchResult> Search(
        IBackofficeSearchRequest request,
        out long totalFound);
}
