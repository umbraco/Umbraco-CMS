using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

internal sealed class NoopExamineManager : IExamineManager
{
    public void Dispose() {}

    public bool TryGetIndex(string indexName, out IIndex index)
    {
        index = null!;
        return false;
    }

    public bool TryGetSearcher(string searcherName, out ISearcher searcher)
    {
        searcher = null!;
        return false;
    }

    public IEnumerable<IIndex> Indexes => Array.Empty<IIndex>();

    public IEnumerable<ISearcher> RegisteredSearchers => Array.Empty<ISearcher>();
}
