using System.Collections;

namespace Umbraco.Cms.Core.Composing;

public sealed class LazyReadOnlyCollection<T> : IReadOnlyCollection<T>
{
    private readonly Lazy<IEnumerable<T>> _lazyCollection;
    private int? _count;

    public LazyReadOnlyCollection(Lazy<IEnumerable<T>> lazyCollection) => _lazyCollection = lazyCollection;

    public LazyReadOnlyCollection(Func<IEnumerable<T>> lazyCollection) =>
        _lazyCollection = new Lazy<IEnumerable<T>>(lazyCollection);

    public IEnumerable<T> Value => EnsureCollection();

    public int Count
    {
        get
        {
            EnsureCollection();
            return _count.GetValueOrDefault();
        }
    }

    public IEnumerator<T> GetEnumerator() => Value.GetEnumerator();

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
