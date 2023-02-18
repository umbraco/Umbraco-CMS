using Umbraco.Cms.Core.Services;

namespace Umbraco.Search.Services;

public class IndexCountService : IIndexCountService
{
    private readonly ISearchProvider _examineManager;

    public IndexCountService(ISearchProvider examineManager) => _examineManager = examineManager;

    public int GetCount() => _examineManager.GetAllIndexes().Count();
}
