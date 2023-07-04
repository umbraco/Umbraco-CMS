using Umbraco.Cms.Core.Collections;

namespace Umbraco.Search.Indexing.Populators;

/// <summary>
///     An <see cref="IIndexPopulator" /> that is automatically associated to any index of type <see cref="TIndex" />
/// </summary>
/// <typeparam name="TIndex"></typeparam>

public abstract class IndexPopulator : IIndexPopulator
{
    private readonly ConcurrentHashSet<string> _registeredIndexes = new();

    public virtual bool IsRegistered(string index) => _registeredIndexes.Contains(index);

    public void Populate(params string[] indexes) => PopulateIndexes(indexes.Where(IsRegistered).ToList());

    /// <summary>
    ///     Registers an index for this populator
    /// </summary>
    /// <param name="indexName"></param>
    public void RegisterIndex(string indexName) => _registeredIndexes.Add(indexName);

    protected abstract void PopulateIndexes(IReadOnlyList<string> indexes);
}
