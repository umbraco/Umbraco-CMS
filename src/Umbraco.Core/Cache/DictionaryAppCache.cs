using System.Collections;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Implements <see cref="IAppCache" /> on top of a concurrent dictionary.
/// </summary>
public class DictionaryAppCache : IRequestCache
{
    /// <summary>
    ///     Gets the internal items dictionary, for tests only!
    /// </summary>
    private readonly ConcurrentDictionary<string, object?> _items = new();

    public int Count => _items.Count;

    /// <inheritdoc />
    public bool IsAvailable => true;

    /// <inheritdoc />
    public virtual object? Get(string key) => _items.TryGetValue(key, out var value) ? value : null;

    /// <inheritdoc />
    public virtual object? Get(string key, Func<object?> factory) => _items.GetOrAdd(key, _ => factory());

    public bool Set(string key, object? value) => _items.TryAdd(key, value);

    public bool Remove(string key) => _items.TryRemove(key, out _);

    /// <inheritdoc />
    public virtual IEnumerable<object?> SearchByKey(string keyStartsWith)
    {
        var items = new List<object?>();
        foreach ((string key, object? value) in _items)
        {
            if (key.InvariantStartsWith(keyStartsWith))
            {
                items.Add(value);
            }
        }

        return items;
    }

    /// <inheritdoc />
    public IEnumerable<object?> SearchByRegex(string regex)
    {
        var compiled = new Regex(regex, RegexOptions.Compiled);
        var items = new List<object?>();
        foreach ((string key, object? value) in _items)
        {
            if (compiled.IsMatch(key))
            {
                items.Add(value);
            }
        }

        return items;
    }

    /// <inheritdoc />
    public virtual void Clear() => _items.Clear();

    /// <inheritdoc />
    public virtual void Clear(string key) => _items.TryRemove(key, out _);

    /// <inheritdoc />
    public virtual void ClearOfType(Type type) =>
        _items.RemoveAll(kvp => kvp.Value != null && kvp.Value.GetType() == type);

    /// <inheritdoc />
    public virtual void ClearOfType<T>()
    {
        Type typeOfT = typeof(T);
        ClearOfType(typeOfT);
    }

    /// <inheritdoc />
    public virtual void ClearOfType<T>(Func<string, T, bool> predicate)
    {
        Type typeOfT = typeof(T);
        _items.RemoveAll(kvp =>
            kvp.Value != null && kvp.Value.GetType() == typeOfT && predicate(kvp.Key, (T)kvp.Value));
    }

    /// <inheritdoc />
    public virtual void ClearByKey(string keyStartsWith) =>
        _items.RemoveAll(kvp => kvp.Key.InvariantStartsWith(keyStartsWith));

    /// <inheritdoc />
    public virtual void ClearByRegex(string regex)
    {
        var compiled = new Regex(regex, RegexOptions.Compiled);
        _items.RemoveAll(kvp => compiled.IsMatch(kvp.Key));
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
