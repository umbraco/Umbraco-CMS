using Examine;

namespace Umbraco.Cms.ManagementApi.Services;

public class ExamineManagerService : IExamineManagerService
{
    private readonly IExamineManager _examineManager;

    public ExamineManagerService(IExamineManager examineManager) => _examineManager = examineManager;

    public bool TryFindSearcher(string searcherName, out ISearcher searcher)
    {
        // try to get the searcher from the indexes
        if (!_examineManager.TryGetIndex(searcherName, out IIndex index))
        {
            // if we didn't find anything try to find it by an explicitly declared searcher
            return _examineManager.TryGetSearcher(searcherName, out searcher);
        }

        searcher = index.Searcher;
        return true;
    }
}
