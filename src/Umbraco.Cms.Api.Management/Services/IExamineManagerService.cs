using Examine;

namespace Umbraco.Cms.Api.Management.Services;

public interface IExamineManagerService
{
    bool TryFindSearcher(string searcherName, out ISearcher searcher);
}
