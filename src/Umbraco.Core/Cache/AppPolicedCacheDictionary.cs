using System.Collections.Concurrent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Provides a base class for implementing a dictionary of <see cref="IAppPolicyCache" />.
/// </summary>
/// <typeparam name="TKey">The type of the dictionary key.</typeparam>
public abstract class AppPolicedCacheDictionary<TKey> : IDisposable
    where TKey : notnull
{
    /// <summary>
    ///     Gets the internal cache factory, for tests only!
    /// </summary>
    private readonly Func<TKey, IAppPolicyCache> _cacheFactory;

    private readonly ConcurrentDictionary<TKey, IAppPolicyCache> _caches = new();
    private bool _disposedValue;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AppPolicedCacheDictionary{TKey}" /> class.
    /// </summary>
    /// <param name="cacheFactory"></param>
    protected AppPolicedCacheDictionary(Func<TKey, IAppPolicyCache> cacheFactory) => _cacheFactory = cacheFactory;

    public void Dispose() =>

        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);

    /// <summary>
    ///     Gets or creates a cache.
    /// </summary>
    public IAppPolicyCache GetOrCreate(TKey key)
        => _caches.GetOrAdd(key, k => _cacheFactory(k));

    /// <summary>
    ///     Removes a cache.
    /// </summary>
    public void Remove(TKey key) => _caches.TryRemove(key, out _);

    /// <summary>
    ///     Removes all caches.
    /// </summary>
    public void RemoveAll() => _caches.Clear();

    /// <summary>
    ///     Clears all caches.
    /// </summary>
    public void ClearAllCaches()
    {
        foreach (IAppPolicyCache cache in _caches.Values)
        {
            cache.Clear();
        }
    }

    /// <summary>
    ///     Tries to get a cache.
    /// </summary>
    protected Attempt<IAppPolicyCache?> Get(TKey key)
        => _caches.TryGetValue(key, out IAppPolicyCache? cache)
            ? Attempt.Succeed(cache)
            : Attempt.Fail<IAppPolicyCache?>();

    /// <summary>
    ///     Clears a cache.
    /// </summary>
    protected void ClearCache(TKey key)
    {
        if (_caches.TryGetValue(key, out IAppPolicyCache? cache))
        {
            cache.Clear();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                foreach (IAppPolicyCache value in _caches.Values)
                {
                    value.DisposeIfDisposable();
                }
            }

            _disposedValue = true;
        }
    }
}
