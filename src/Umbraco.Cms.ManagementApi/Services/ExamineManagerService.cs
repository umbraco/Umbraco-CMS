using Examine;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.ManagementApi.Services;

public class ExamineManagerService : IExamineManagerService
{
    private readonly IExamineManager _examineManager;
    private readonly IIndexRebuilder _indexRebuilder;

    public ExamineManagerService(IExamineManager examineManager, IIndexRebuilder indexRebuilder)
    {
        _examineManager = examineManager;
        _indexRebuilder = indexRebuilder;
    }

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

    public bool ValidateIndex(string indexName, out IIndex? index)
    {
        index = null;

        return _examineManager.TryGetIndex(indexName, out index);
    }

    public bool ValidatePopulator(IIndex index) => _indexRebuilder.CanRebuild(index.Name);
}
