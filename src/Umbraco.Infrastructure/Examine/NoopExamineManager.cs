using Examine;

namespace Umbraco.Cms.Infrastructure.Examine;

internal sealed class NoopExamineManager : IExamineManager
{
    /// <summary>
    /// Releases resources used by the <see cref="NoopExamineManager"/>. This implementation does nothing.
    /// </summary>
    public void Dispose() {}

    /// <summary>
    /// Attempts to get an index by name.
    /// </summary>
    /// <param name="indexName">The name of the index to retrieve.</param>
    /// <param name="index">When this method returns, contains the index associated with the specified name, if the index is found; otherwise, null.</param>
    /// <returns><c>true</c> if the index was found; otherwise, <c>false</c>.</returns>
    public bool TryGetIndex(string indexName, out IIndex index)
    {
        index = null!;
        return false;
    }

    /// <summary>
    /// Attempts to get a searcher by name.
    /// </summary>
    /// <param name="searcherName">The name of the searcher to retrieve.</param>
    /// <param name="searcher">When this method returns, contains the searcher associated with the specified name, if the searcher is found; otherwise, null.</param>
    /// <returns><c>true</c> if the searcher was found; otherwise, <c>false</c>.</returns>
    public bool TryGetSearcher(string searcherName, out ISearcher searcher)
    {
        searcher = null!;
        return false;
    }

    /// <summary>
    /// Gets an empty collection of indexes.
    /// </summary>
    public IEnumerable<IIndex> Indexes => Array.Empty<IIndex>();

    /// <summary>
    /// Gets a collection of registered searchers, which is always empty for the <see cref="NoopExamineManager"/>.
    /// </summary>
    public IEnumerable<ISearcher> RegisteredSearchers => Array.Empty<ISearcher>();
}
