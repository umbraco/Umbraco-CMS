using Examine;

namespace Umbraco.Cms.ManagementApi.Services;

public interface IExamineManagerService
{
    bool TryFindSearcher(string searcherName, out ISearcher searcher);

    bool ValidateIndex(string indexName, out IIndex? index);

    bool ValidatePopulator(IIndex index);
}
