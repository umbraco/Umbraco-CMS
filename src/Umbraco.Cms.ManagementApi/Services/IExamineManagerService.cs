using Examine;

namespace Umbraco.Cms.ManagementApi.Services;

public interface IExamineManagerService
{
    bool TryFindSearcher(string searcherName, out ISearcher searcher);
}
