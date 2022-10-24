using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Deploy;

/// <summary>
/// A context cache that stores items in a dictionary.
/// </summary>
/// <seealso cref="Umbraco.Cms.Core.Deploy.IContextCache" />
public class DictionaryCache : IContextCache
{
    /// <summary>
    /// The items.
    /// </summary>
    protected readonly IDictionary<string, object> _items;

    /// <summary>
    /// The prefix.
    /// </summary>
    protected readonly string? _prefix;

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryCache" /> class.
    /// </summary>
    public DictionaryCache()
        : this(new Dictionary<string, object>(), null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryCache" /> class.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <param name="prefix">The prefix to add to all keys.</param>
    /// <exception cref="System.ArgumentNullException">items</exception>
    protected DictionaryCache(IDictionary<string, object> items, string? prefix)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
        _prefix = prefix;
    }

    /// <inheritdoc />
    public void Create<T>(string key, T item)
    {
        var prefixedKey = _prefix + key;
        if (item != null)
        {
            _items[prefixedKey] = item;
        }
    }

    /// <inheritdoc />
    public T? GetOrCreate<T>(string key, Func<T?> factory)
    {
        var prefixedKey = _prefix + key;
        if (_items.TryGetValue(prefixedKey, out var itemValue) &&
           itemValue is T item)
        {
            return item;
        }

        var factoryItem = factory();
        if (factoryItem != null)
        {
            _items[prefixedKey] = factoryItem;
            return factoryItem;
        }            

        return default;
    }

    /// <inheritdoc />
    public void Clear()
    {
        if (string.IsNullOrEmpty(_prefix))
        {
            _items.Clear();
        }
        else
        {
            _items.RemoveAll(x => x.Key.StartsWith(_prefix, StringComparison.Ordinal));
        }
    }
}
