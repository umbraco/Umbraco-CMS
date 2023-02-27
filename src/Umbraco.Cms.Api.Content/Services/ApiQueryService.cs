using Examine;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.ContentApi;

namespace Umbraco.Cms.Api.Content.Services;
public class ApiQueryService : IApiQueryService
{
    private readonly IExamineManager _examineManager;

    public ApiQueryService(IExamineManager examineManager) => _examineManager = examineManager;

    public IEnumerable<Guid> GetChildrenIds(Guid id)
    {
        if (!_examineManager.TryGetIndex(Constants.UmbracoIndexes.ContentAPIIndexName, out IIndex? apiIndex))
        {
            return Enumerable.Empty<Guid>(); // Return attempt?
        }

        ISearcher searcher = apiIndex.Searcher;
        ISearchResults results = searcher
            .CreateQuery()
            .Field("parentKey", id.ToString())
            .Execute();

        return results.Select(x => Guid.Parse(x.Id));
    }
}
