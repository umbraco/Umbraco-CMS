using System.Collections;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
/// Represents a lazily-initialized read-only collection.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public sealed class LazyReadOnlyCollection<T> : IReadOnlyCollection<T>
{
    private readonly Lazy<IEnumerable<T>> _lazyCollection;
    private int? _count;

    /// <summary>
    /// Initializes a new instance of the <see cref="LazyReadOnlyCollection{T}" /> class.
    /// </summary>
    /// <param name="lazyCollection">A lazy wrapper containing the collection factory.</param>
    public LazyReadOnlyCollection(Lazy<IEnumerable<T>> lazyCollection) => _lazyCollection = lazyCollection;

    /// <summary>
    /// Initializes a new instance of the <see cref="LazyReadOnlyCollection{T}" /> class.
    /// </summary>
    /// <param name="lazyCollection">A factory function that creates the collection when first accessed.</param>
    public LazyReadOnlyCollection(Func<IEnumerable<T>> lazyCollection) =>
        _lazyCollection = new Lazy<IEnumerable<T>>(lazyCollection);

    /// <summary>
    /// Gets the underlying collection value.
    /// </summary>
    public IEnumerable<T> Value => EnsureCollection();

    /// <inheritdoc />
    public int Count
    {
        get
        {
            EnsureCollection();
            return _count.GetValueOrDefault();
        }
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => Value.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private IEnumerable<T> EnsureCollection()
    {
        if (_lazyCollection == null)
        {
            _count = 0;
            return Enumerable.Empty<T>();
        }

        IEnumerable<T> val = _lazyCollection.Value;
        if (_count == null)
        {
            _count = val.Count();
        }

        return val;
    }
}
