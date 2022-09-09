using Examine;

namespace Umbraco.Cms.ManagementApi.Services;

public class ExamineSearcherValidationService : IExamineSearcherValidationService
{
    private readonly IExamineManager _examineManager;

    public ExamineSearcherValidationService(IExamineManager examineManager)
    {
        _examineManager = examineManager;
    }

    public bool ValidateSearcher(string searcherName, out ISearcher searcher)
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
