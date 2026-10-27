using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Implements <see cref="IAppPolicyCache" /> on top of a <see cref="MemoryCache" />.
/// </summary>
public class ObjectCacheAppCache : IAppPolicyCache, IDisposable
{
    private readonly Dictionary<string, Lazy<object?>> _entries = new();
    private readonly ReaderWriterLockSlim _locker = new(LockRecursionPolicy.SupportsRecursion);
    private bool _disposedValue;
    private int _completedEvictionCallbacks;

    private static readonly TimeSpan _readLockTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan _writeLockTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets the internal memory cache, for tests only!
    /// </summary>
    /// <value>
    /// The memory cache.
    /// </value>
    internal MemoryCache MemoryCache { get; private set; }

    /// <summary>
    /// Gets the number of post-eviction callbacks that have completed.
    /// </summary>
    /// <remarks>
    /// Marked as internal to allow access for tests. Eviction callbacks run on a background thread, 
    /// so tests use this to wait for them deterministically.
    /// </remarks>
    internal int CompletedEvictionCallbacks => Volatile.Read(ref _completedEvictionCallbacks);

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectCacheAppCache" />.
    /// </summary>
    public ObjectCacheAppCache()
        : this(new MemoryCacheOptions(), NullLoggerFactory.Instance)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectCacheAppCache" /> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    internal ObjectCacheAppCache(IOptions<MemoryCacheOptions> options, ILoggerFactory loggerFactory)
        => MemoryCache = new MemoryCache(options, loggerFactory);

    /// <inheritdoc />
    public object? Get(string key)
    {
        Lazy<object?>? result;
        try
        {
            if (_locker.TryEnterReadLock(_readLockTimeout) is false)
            {
                throw new TimeoutException("Timeout exceeded to the memory cache when getting item by key.");
            }

            result = MemoryCache.Get(key) as Lazy<object?>; // null if key not found
        }
        finally
        {
            if (_locker.IsReadLockHeld)
            {
                _locker.ExitReadLock();
            }
        }

        return result is null
            ? null
            : SafeLazy.GetSafeLazyValue(result); // return exceptions as null
    }

    /// <inheritdoc />
    public object? Get(string key, Func<object?> factory) => Get(key, factory, null);

    /// <inheritdoc />
    public IEnumerable<object> SearchByKey(string keyStartsWith) => SearchByPredicate(key => key.InvariantStartsWith(keyStartsWith));

    /// <inheritdoc />
    public IEnumerable<object> SearchByRegex(string regex) => SearchByPredicate(new Regex(regex, RegexOptions.Compiled).IsMatch);

    private IEnumerable<object> SearchByPredicate(Func<string, bool> predicate)
    {
        object[] entries;
        try
        {
            if (_locker.TryEnterReadLock(_readLockTimeout) is false)
            {
                throw new TimeoutException("Timeout exceeded to the memory cache when searching items by predicate.");
            }

            entries = _entries.Keys.Where(predicate)
                .Select(MemoryCache.Get)
                .WhereNotNull()
                .ToArray(); // evaluate while locked
        }
        finally
        {
            if (_locker.IsReadLockHeld)
            {
                _locker.ExitReadLock();
            }
        }

        return entries
            .Select(x => SafeLazy.GetSafeLazyValue((Lazy<object?>)x)) // return exceptions as null
            .WhereNotNull() // backward compat, don't store null values in the cache
            .ToList();
    }

    /// <inheritdoc />
    public object? Get(string key, Func<object?> factory, TimeSpan? timeout, bool isSliding = false)
    {
        // see notes in HttpRuntimeAppCache
        Lazy<object?>? result;
        try
        {
            if (_locker.TryEnterUpgradeableReadLock(_readLockTimeout) is false)
            {
                throw new TimeoutException("Timeout exceeded to the memory cache when getting item by key.");
            }

            result = MemoryCache.Get(key) as Lazy<object?>;

            // get non-created as NonCreatedValue & exceptions as null
            if (result is null || SafeLazy.GetSafeLazyValue(result, true) is null)
            {
                result = SafeLazy.GetSafeLazy(factory);

                try
                {
                    if (_locker.TryEnterWriteLock(_writeLockTimeout) is false)
                    {
                        throw new TimeoutException("Timeout exceeded to the memory cache when inserting item.");
                    }

                    // NOTE: This does an add or update
                    MemoryCache.Set(key, result, GetOptions(timeout, isSliding));
                    _entries[key] = result;
                }
                finally
                {
                    if (_locker.IsWriteLockHeld)
                    {
                        _locker.ExitWriteLock();
                    }
                }
            }
        }
        finally
        {
            if (_locker.IsUpgradeableReadLockHeld)
            {
                _locker.ExitUpgradeableReadLock();
            }
        }

        var value = result.Value; // will not throw (safe lazy)
        if (value is SafeLazy.ExceptionHolder eh)
        {
            eh.Exception.Throw(); // throw once!
        }

        return value;
    }

    /// <inheritdoc />
    public void Insert(string key, Func<object?> factory, TimeSpan? timeout = null, bool isSliding = false)
    {
        // NOTE - here also we must insert a Lazy<object> but we can evaluate it right now
        // and make sure we don't store a null value.
        Lazy<object?> result = SafeLazy.GetSafeLazy(factory);
        var value = result.Value; // force evaluation now
        if (value is null)
        {
            return; // do not store null values (backward compat)
        }

        try
        {
            if (_locker.TryEnterWriteLock(_writeLockTimeout) is false)
            {
                throw new TimeoutException("Timeout exceeded to the memory cache when inserting item.");
            }

            // NOTE: This does an add or update
            MemoryCache.Set(key, result, GetOptions(timeout, isSliding));
            _entries[key] = result;
        }
        finally
        {
            if (_locker.IsWriteLockHeld)
            {
                _locker.ExitWriteLock();
            }
        }
    }

    /// <inheritdoc />
    public virtual void Clear()
    {
        try
        {
            if (_locker.TryEnterWriteLock(_writeLockTimeout) is false)
            {
                throw new TimeoutException("Timeout exceeded to the memory cache when clearing all items.");
            }

            MemoryCache.Clear();
            _entries.Clear();
        }
        finally
        {
            if (_locker.IsWriteLockHeld)
            {
                _locker.ExitWriteLock();
            }
        }
    }

    /// <inheritdoc />
    public virtual void Clear(string key)
    {
        try
        {
            if (_locker.TryEnterWriteLock(_writeLockTimeout) is false)
            {
                throw new TimeoutException("Timeout exceeded to the memory cache when clearing item by key.");
            }

            MemoryCache.Remove(key);
            _entries.Remove(key);
        }
        finally
        {
            if (_locker.IsWriteLockHeld)
            {
                _locker.ExitWriteLock();
            }
        }
    }

    /// <inheritdoc />
    public virtual void ClearOfType(Type type)
    {
        if (type == null)
        {
            return;
        }

        var isInterface = type.IsInterface;

        ClearByPredicate(key =>
        {
            var entry = MemoryCache.Get(key);
            if (entry is null)
            {
                return false;
            }

            // x.Value is Lazy<object> and not null, its value may be null
            // remove null values as well, does not hurt
            // get non-created as NonCreatedValue & exceptions as null
            var value = SafeLazy.GetSafeLazyValue((Lazy<object?>)entry, true);

            // if T is an interface remove anything that implements that interface
            // otherwise remove exact types (not inherited types)
            return value == null || (isInterface ? type.IsInstanceOfType(value) : value.GetType() == type);
        });
    }

    /// <inheritdoc />
    public virtual void ClearOfType<T>() => ClearOfType(typeof(T));

    /// <inheritdoc />
    public virtual void ClearOfType<T>(Func<string, T, bool> predicate)
    {
        Type type = typeof(T);
        var isInterface = type.IsInterface;

        ClearByPredicate(key =>
        {
            var entry = MemoryCache.Get(key);
            if (entry is null)
            {
                return false;
            }

            // x.Value is Lazy<object> and not null, its value may be null
            // remove null values as well, does not hurt
            // get non-created as NonCreatedValue & exceptions as null
            var value = SafeLazy.GetSafeLazyValue((Lazy<object?>)entry, true);
            if (value == null)
            {
                return true;
            }

            // if T is an interface remove anything that implements that interface
            // otherwise remove exact types (not inherited types)
            return (isInterface ? value is T : value.GetType() == type) && predicate(key, (T)value);
        });
    }

    /// <inheritdoc />
    public virtual void ClearByKey(string keyStartsWith) => ClearByPredicate(x => x.InvariantStartsWith(keyStartsWith));

    /// <inheritdoc />
    public virtual void ClearByRegex(string regex) => ClearByPredicate(new Regex(regex, RegexOptions.Compiled).IsMatch);

    private void ClearByPredicate(Func<string, bool> predicate)
    {
        try
        {
            if (_locker.TryEnterWriteLock(_writeLockTimeout) is false)
            {
                throw new TimeoutException("Timeout exceeded to the memory cache when clearing items by predicate.");
            }

            // ToArray required to remove
            foreach (var key in _entries.Keys.Where(predicate).ToArray())
            {
                MemoryCache.Remove(key);
                _entries.Remove(key);
            }
        }
        finally
        {
            if (_locker.IsWriteLockHeld)
            {
                _locker.ExitWriteLock();
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        => Dispose(true);

    /// <summary>
    ///     Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _locker.Dispose();
                MemoryCache.Dispose();
            }

            _disposedValue = true;
        }
    }

    private MemoryCacheEntryOptions GetOptions(TimeSpan? timeout, bool isSliding)
    {
        var options = new MemoryCacheEntryOptions();

        // Configure time based expiration
        if (isSliding)
        {
            options.SlidingExpiration = timeout;
        }
        else
        {
            options.AbsoluteExpirationRelativeToNow = timeout;
        }

        // Ensure key is no longer tracked when evicted from cache
        return options.RegisterPostEvictionCallback((key, value, reason, _) =>
        {
            try
            {
                PruneEvictedEntry((string)key, value, reason);
            }
            finally
            {
                Interlocked.Increment(ref _completedEvictionCallbacks);
            }
        });
    }

    private void PruneEvictedEntry(string key, object? value, EvictionReason reason)
    {
        // Removed and Replaced evictions don't need pruning here: the Remove/Clear call sites already
        // prune the tracked entries synchronously under the write lock, and a Replaced key still has a
        // live entry (the synchronous Set re-added it).
        if (reason is EvictionReason.Removed or EvictionReason.Replaced)
        {
            return;
        }

        try
        {
            if (_locker.TryEnterWriteLock(_writeLockTimeout) is false)
            {
                throw new TimeoutException("Timeout exceeded to the memory cache when removing key.");
            }

            // The callback runs on a background thread, so the key may have been re-cached with a new
            // entry since this one was evicted (e.g. an expired entry re-read through Get with a factory).
            // Only prune the key while it still tracks the evicted entry. Every write stores a new Lazy, so a
            // reference check identifies the entry even when the factory returns the same instance each time.
            if (_entries.TryGetValue(key, out var tracked) && ReferenceEquals(tracked, value))
            {
                _entries.Remove(key);
            }
        }
        finally
        {
            if (_locker.IsWriteLockHeld)
            {
                _locker.ExitWriteLock();
            }
        }
    }
}
