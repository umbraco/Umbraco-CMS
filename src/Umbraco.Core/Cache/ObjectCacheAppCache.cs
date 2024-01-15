using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Implements <see cref="IAppPolicyCache" /> on top of a <see cref="MemoryCache" />.
/// </summary>
public class ObjectCacheAppCache : IAppPolicyCache, IDisposable
{
    private readonly IOptions<MemoryCacheOptions> _options;
    private readonly IHostEnvironment? _hostEnvironment;
    private readonly ISet<string> _keys = new HashSet<string>();
    private readonly ReaderWriterLockSlim _locker = new(LockRecursionPolicy.SupportsRecursion);
    private bool _disposedValue;

    /// <summary>
    /// Gets the internal memory cache, for tests only!
    /// </summary>
    /// <value>
    /// The memory cache.
    /// </value>
    internal MemoryCache MemoryCache { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectCacheAppCache" />.
    /// </summary>
    public ObjectCacheAppCache()
        : this(Options.Create(new MemoryCacheOptions()), NullLoggerFactory.Instance, null)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectCacheAppCache" /> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="hostEnvironment">The host environment.</param>
    public ObjectCacheAppCache(IOptions<MemoryCacheOptions> options, ILoggerFactory loggerFactory, IHostEnvironment? hostEnvironment)
    {
        _options = options;
        _hostEnvironment = hostEnvironment;

        MemoryCache = new MemoryCache(_options, loggerFactory);
    }

    /// <inheritdoc />
    public object? Get(string key)
    {
        Lazy<object?>? result;
        try
        {
            _locker.EnterReadLock();

            result = MemoryCache.Get(key) as Lazy<object?>; // null if key not found
        }
        finally
        {
            if (_locker.IsReadLockHeld)
            {
                _locker.ExitReadLock();
            }
        }

        return result == null ? null : SafeLazy.GetSafeLazyValue(result); // return exceptions as null
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
            _locker.EnterReadLock();

            entries = _keys.Where(predicate)
                .Select(key => MemoryCache.Get(key))
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
            _locker.EnterUpgradeableReadLock();

            result = MemoryCache.Get(key) as Lazy<object?>;

            // get non-created as NonCreatedValue & exceptions as null
            if (result == null || SafeLazy.GetSafeLazyValue(result, true) == null)
            {
                result = SafeLazy.GetSafeLazy(factory);
                MemoryCacheEntryOptions options = GetOptions(timeout, isSliding);

                try
                {
                    _locker.EnterWriteLock();

                    // NOTE: This does an add or update
                    MemoryCache.Set(key, result, options);
                    _keys.Add(key);
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

        // return result.Value;
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
        if (value == null)
        {
            return; // do not store null values (backward compat)
        }

        MemoryCacheEntryOptions options = GetOptions(timeout, isSliding);

        // NOTE: This does an add or update
        MemoryCache.Set(key, result, options);
        _keys.Add(key);
    }

    /// <inheritdoc />
    public virtual void Clear()
    {
        try
        {
            _locker.EnterWriteLock();

            MemoryCache.Dispose();
            MemoryCache = new MemoryCache(_options);
            _keys.Clear();
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
            _locker.EnterWriteLock();

            MemoryCache.Remove(key);
            _keys.Remove(key);
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
            _locker.EnterWriteLock();

            // ToArray required to remove
            foreach (var key in _keys.Where(predicate).ToArray())
            {
                MemoryCache.Remove(key);
                _keys.Remove(key);
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

    public void Dispose() =>
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);

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

    private MemoryCacheEntryOptions GetOptions(TimeSpan? timeout = null, bool isSliding = false)
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

        // Ensure key is removed from set when evicted from cache
        return options.RegisterPostEvictionCallback((key, _, _, _) => _keys.Remove((string)key));
    }
}
