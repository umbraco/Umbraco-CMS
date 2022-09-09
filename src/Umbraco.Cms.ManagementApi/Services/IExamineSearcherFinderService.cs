using Examine;

namespace Umbraco.Cms.ManagementApi.Services;

public interface IExamineSearcherFinderService
{
    bool TryFindSearcher(string searcherName, out ISearcher searcher);
}
