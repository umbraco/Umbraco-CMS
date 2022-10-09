using System.Collections;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Provides a base class for builder collections.
/// </summary>
/// <typeparam name="TItem">The type of the items.</typeparam>
public abstract class BuilderCollectionBase<TItem> : IBuilderCollection<TItem>
{
    private readonly LazyReadOnlyCollection<TItem> _items;

    /// Initializes a new instance of the
    /// <see cref="BuilderCollectionBase{TItem}" />
    /// with items.
    /// </summary>
    /// <param name="items">The items.</param>
    public BuilderCollectionBase(Func<IEnumerable<TItem>> items) => _items = new LazyReadOnlyCollection<TItem>(items);

    /// <inheritdoc />
    public int Count => _items.Count;

    /// <summary>
    ///     Gets an enumerator.
    /// </summary>
    public IEnumerator<TItem> GetEnumerator() => _items.GetEnumerator();

    /// <summary>
    ///     Gets an enumerator.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
