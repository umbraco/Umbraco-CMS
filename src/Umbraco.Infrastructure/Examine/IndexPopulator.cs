using Examine;
using Umbraco.Cms.Core.Collections;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     An <see cref="IIndexPopulator" /> that is automatically associated to any index of type <typeparamref name="TIndex"/>
/// </summary>
/// <typeparam name="TIndex"></typeparam>
public abstract class IndexPopulator<TIndex> : IndexPopulator
    where TIndex : IIndex
{
    /// <summary>
    /// Determines whether the specified index is registered.
    /// </summary>
    /// <param name="index">The index to check for registration.</param>
    /// <returns>True if the index is registered; otherwise, false.</returns>
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

    /// <summary>Determines whether the specified index is registered.</summary>
    /// <param name="index">The index to check for registration.</param>
    /// <returns><c>true</c> if the index is registered; otherwise, <c>false</c>.</returns>
    public virtual bool IsRegistered(TIndex index) => true;
}

/// <summary>
/// Provides functionality to populate and manage Examine indexes with data in Umbraco.
/// </summary>
public abstract class IndexPopulator : IIndexPopulator
{
    private readonly ConcurrentHashSet<string> _registeredIndexes = new();

    /// <summary>Determines whether the specified index is registered.</summary>
    /// <param name="index">The index to check for registration.</param>
    /// <returns>True if the index is registered; otherwise, false.</returns>
    public virtual bool IsRegistered(IIndex index) => _registeredIndexes.Contains(index.Name);

    /// <summary>
    /// Populates the specified indexes by initializing or updating their contents as required.
    /// Only indexes that are registered will be populated.
    /// </summary>
    /// <param name="indexes">The indexes to populate.</param>
    public void Populate(params IIndex[] indexes) => PopulateIndexes(indexes.Where(IsRegistered).ToList());

    /// <summary>
    ///     Registers an index for this populator
    /// </summary>
    /// <param name="indexName"></param>
    public void RegisterIndex(string indexName) => _registeredIndexes.Add(indexName);

    protected abstract void PopulateIndexes(IReadOnlyList<IIndex> indexes);
}
