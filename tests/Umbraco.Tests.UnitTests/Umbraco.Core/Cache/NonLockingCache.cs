// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

/// <summary>
/// A simple non-thread-safe cache for testing purposes.
/// This allows race conditions to manifest that would be hidden by ObjectCacheAppCache's internal locking.
/// </summary>
internal sealed class NonLockingCache : IAppPolicyCache
{
    private readonly Dictionary<string, object?> _cache = [];

    public object? Get(string key)
        => _cache.TryGetValue(key, out var value) ? value : null;

    public object? Get(string key, Func<object?> factory) => Get(key, factory, null, false);

    public object? Get(string key, Func<object?> factory, TimeSpan? timeout, bool isSliding = false)
    {
        if (_cache.TryGetValue(key, out var value))
        {
            return value;
        }

        value = factory();
        _cache[key] = value;
        return value;
    }

    public IEnumerable<object> SearchByKey(string keyStartsWith)
        => _cache.Where(kvp => kvp.Key.StartsWith(keyStartsWith, StringComparison.InvariantCultureIgnoreCase)).Select(kvp => kvp.Value!);

    public IEnumerable<object> SearchByRegex(string regex) => throw new NotImplementedException();

    public void Clear() => _cache.Clear();

    public void Clear(string key) => _cache.Remove(key);

    public void ClearOfType(Type type) => throw new NotImplementedException();

    public void ClearOfType<T>() => throw new NotImplementedException();

    public void ClearOfType<T>(Func<string, T, bool> predicate) => throw new NotImplementedException();

    public void ClearByKey(string keyStartsWith)
    {
        var keysToRemove = _cache.Keys.Where(k => k.StartsWith(keyStartsWith, StringComparison.InvariantCultureIgnoreCase)).ToList();
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
        }
    }

    public void ClearByRegex(string regex) => throw new NotImplementedException();

    public void Insert(string key, Func<object?> factory, TimeSpan? timeout = null, bool isSliding = false)
        => _cache[key] = factory();
}
