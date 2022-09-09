using Examine;

namespace Umbraco.Cms.ManagementApi.Services;

public interface IExamineSearcherValidationService
{
    bool TryFindSearcher(string searcherName, out ISearcher searcher);
}
