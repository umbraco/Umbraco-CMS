using Examine;

namespace Umbraco.Cms.ManagementApi.Services;

public interface IExamineSearcherValidationService
{
    bool ValidateSearcher(string searcherName, out ISearcher searcher);
}
