using Examine;
using Umbraco.Cms.Core.Collections;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     An <see cref="IIndexPopulator" /> that is automatically associated to any index of type <see cref="TIndex" />
/// </summary>
/// <typeparam name="TIndex"></typeparam>
public abstract class IndexPopulator<TIndex> : IndexPopulator
    where TIndex : IIndex
{
    public override bool IsRegistered(IIndex index)
    {
        if (base.IsRegistered(index))
        {
            return true;
        }

        if (!(index is TIndex casted))
        {
            return false;
        }

        return IsRegistered(casted);
    }

    public virtual bool IsRegistered(TIndex index) => true;
}

public abstract class IndexPopulator : IIndexPopulator
{
    private readonly ConcurrentHashSet<string> _registeredIndexes = new();

    public virtual bool IsRegistered(IIndex index) => _registeredIndexes.Contains(index.Name);

    public void Populate(params IIndex[] indexes) => PopulateIndexes(indexes.Where(IsRegistered).ToList());

    /// <summary>
    ///     Registers an index for this populator
    /// </summary>
    /// <param name="indexName"></param>
    public void RegisterIndex(string indexName) => _registeredIndexes.Add(indexName);

    protected abstract void PopulateIndexes(IReadOnlyList<IIndex> indexes);
}
