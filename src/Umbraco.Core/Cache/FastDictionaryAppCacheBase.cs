using System.Text.RegularExpressions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Provides a base class to fast, dictionary-based <see cref="IAppCache" /> implementations.
/// </summary>
public abstract class FastDictionaryAppCacheBase : IAppCache
{
    /// <summary>
    ///     The prefix used for all cache keys to distinguish them from other cache entries.
    /// </summary>
    protected const string CacheItemPrefix = "umbrtmche";

    #region IAppCache

    /// <inheritdoc />
    public virtual object? Get(string key)
    {
        key = GetCacheKey(key);
        Lazy<object?>? result;
        try
        {
            EnterReadLock();
            result = GetEntry(key) as Lazy<object?>; // null if key not found
        }
        finally
        {
            ExitReadLock();
        }

        return result == null ? null : SafeLazy.GetSafeLazyValue(result); // return exceptions as null
    }

    /// <inheritdoc />
    public abstract object? Get(string key, Func<object?> factory);

    /// <inheritdoc />
    public virtual IEnumerable<object> SearchByKey(string keyStartsWith)
    {
        var plen = CacheItemPrefix.Length + 1;
        IEnumerable<KeyValuePair<object, object>> entries;
        try
        {
            EnterReadLock();
            entries = GetDictionaryEntries()
                .Where(x => ((string)x.Key).Substring(plen).InvariantStartsWith(keyStartsWith))
                .ToArray(); // evaluate while locked
        }
        finally
        {
            ExitReadLock();
        }

        return entries
            .Select(x => SafeLazy.GetSafeLazyValue((Lazy<object?>)x.Value)) // return exceptions as null
            .Where(x => x != null)!; // backward compat, don't store null values in the cache
    }

    /// <inheritdoc />
    public virtual IEnumerable<object?> SearchByRegex(string regex)
    {
        const string prefix = CacheItemPrefix + "-";
        var compiled = new Regex(regex, RegexOptions.Compiled);
        var plen = prefix.Length;
        IEnumerable<KeyValuePair<object, object>> entries;
        try
        {
            EnterReadLock();
            entries = GetDictionaryEntries()
                .Where(x => compiled.IsMatch(((string)x.Key).Substring(plen)))
                .ToArray(); // evaluate while locked
        }
        finally
        {
            ExitReadLock();
        }

        return entries
            .Select(x => SafeLazy.GetSafeLazyValue((Lazy<object?>)x.Value)) // return exceptions as null
            .Where(x => x != null); // backward compatible, don't store null values in the cache
    }

    /// <inheritdoc />
    public virtual void Clear()
    {
        try
        {
            EnterWriteLock();
            foreach (KeyValuePair<object, object> entry in GetDictionaryEntries().ToArray())
            {
                RemoveEntry((string)entry.Key);
            }
        }
        finally
        {
            ExitWriteLock();
        }
    }

    /// <inheritdoc />
    public virtual void Clear(string key)
    {
        var cacheKey = GetCacheKey(key);
        try
        {
            EnterWriteLock();
            RemoveEntry(cacheKey);
        }
        finally
        {
            ExitWriteLock();
        }
    }

    /// <inheritdoc />
    public virtual void ClearOfType(Type? type)
    {
        if (type == null)
        {
            return;
        }

        var isInterface = type.IsInterface;
        try
        {
            EnterWriteLock();
            foreach (KeyValuePair<object, object> entry in GetDictionaryEntries()
                         .Where(x =>
                         {
                             // entry.Value is Lazy<object> and not null, its value may be null
                             // remove null values as well, does not hurt
                             // get non-created as NonCreatedValue & exceptions as null
                             var value = SafeLazy.GetSafeLazyValue((Lazy<object?>)x.Value, true);

                             // if T is an interface remove anything that implements that interface
                             // otherwise remove exact types (not inherited types)
                             return value == null ||
                                    (isInterface ? type.IsInstanceOfType(value) : value.GetType() == type);
                         })
                         .ToArray())
            {
                RemoveEntry((string)entry.Key);
            }
        }
        finally
        {
            ExitWriteLock();
        }
    }

    /// <inheritdoc />
    public virtual void ClearOfType<T>()
    {
        Type typeOfT = typeof(T);
        var isInterface = typeOfT.IsInterface;
        try
        {
            EnterWriteLock();
            foreach (KeyValuePair<object, object> entry in GetDictionaryEntries()
                         .Where(x =>
                         {
                             // entry.Value is Lazy<object> and not null, its value may be null
                             // remove null values as well, does not hurt
                             // compare on exact type, don't use "is"
                             // get non-created as NonCreatedValue & exceptions as null
                             var value = SafeLazy.GetSafeLazyValue((Lazy<object?>)x.Value, true);

                             // if T is an interface remove anything that implements that interface
                             // otherwise remove exact types (not inherited types)
                             return value == null || (isInterface ? value is T : value.GetType() == typeOfT);
                         })
                         .ToArray())
            {
                RemoveEntry((string)entry.Key);
            }
        }
        finally
        {
            ExitWriteLock();
        }
    }

    /// <inheritdoc />
    public virtual void ClearOfType<T>(Func<string, T, bool> predicate)
    {
        Type typeOfT = typeof(T);
        var isInterface = typeOfT.IsInterface;
        var plen = CacheItemPrefix.Length + 1;
        try
        {
            EnterWriteLock();
            foreach (KeyValuePair<object, object> entry in GetDictionaryEntries()
                         .Where(x =>
                         {
                             // entry.Value is Lazy<object> and not null, its value may be null
                             // remove null values as well, does not hurt
                             // compare on exact type, don't use "is"
                             // get non-created as NonCreatedValue & exceptions as null
                             var value = SafeLazy.GetSafeLazyValue((Lazy<object?>)x.Value, true);
                             if (value == null)
                             {
                                 return true;
                             }

                             // if T is an interface remove anything that implements that interface
                             // otherwise remove exact types (not inherited types)
                             return (isInterface ? value is T : value.GetType() == typeOfT)

                                    // run predicate on the 'public key' part only, ie without prefix
                                    && predicate(((string) x.Key).Substring(plen), (T) value);
                         }))
            {
                RemoveEntry((string)entry.Key);
            }
        }
        finally
        {
            ExitWriteLock();
        }
    }

    /// <inheritdoc />
    public virtual void ClearByKey(string keyStartsWith)
    {
        var plen = CacheItemPrefix.Length + 1;
        try
        {
            EnterWriteLock();
            foreach (KeyValuePair<object, object> entry in GetDictionaryEntries()
                         .Where(x => ((string)x.Key).Substring(plen).InvariantStartsWith(keyStartsWith))
                         .ToArray())
            {
                RemoveEntry((string)entry.Key);
            }
        }
        finally
        {
            ExitWriteLock();
        }
    }

    /// <inheritdoc />
    public virtual void ClearByRegex(string regex)
    {
        var compiled = new Regex(regex, RegexOptions.Compiled);
        var plen = CacheItemPrefix.Length + 1;
        try
        {
            EnterWriteLock();
            foreach (KeyValuePair<object, object> entry in GetDictionaryEntries()
                         .Where(x => compiled.IsMatch(((string)x.Key).Substring(plen)))
                         .ToArray())
            {
                RemoveEntry((string)entry.Key);
            }
        }
        finally
        {
            ExitWriteLock();
        }
    }

    #endregion

    #region Dictionary

    /// <summary>
    ///     Gets all dictionary entries from the underlying cache.
    /// </summary>
    /// <returns>The dictionary entries.</returns>
    /// <remarks>Must be called from within the appropriate locks.</remarks>
    protected abstract IEnumerable<KeyValuePair<object, object>> GetDictionaryEntries();

    /// <summary>
    ///     Removes an entry from the underlying cache.
    /// </summary>
    /// <param name="key">The full prefixed cache key.</param>
    /// <remarks>Must be called from within the appropriate locks.</remarks>
    protected abstract void RemoveEntry(string key);

    /// <summary>
    ///     Gets an entry from the underlying cache.
    /// </summary>
    /// <param name="key">The full prefixed cache key.</param>
    /// <returns>The cached value, or <c>null</c> if not found.</returns>
    /// <remarks>Must be called from within the appropriate locks.</remarks>
    protected abstract object? GetEntry(string key);

    /// <summary>
    ///     Enters a read lock on the underlying cache.
    /// </summary>
    protected abstract void EnterReadLock();

    /// <summary>
    ///     Exits the read lock on the underlying cache.
    /// </summary>
    protected abstract void ExitReadLock();

    /// <summary>
    ///     Enters a write lock on the underlying cache.
    /// </summary>
    protected abstract void EnterWriteLock();

    /// <summary>
    ///     Exits the write lock on the underlying cache.
    /// </summary>
    protected abstract void ExitWriteLock();

    /// <summary>
    ///     Gets the prefixed cache key.
    /// </summary>
    /// <param name="key">The public cache key.</param>
    /// <returns>The full prefixed cache key.</returns>
    protected string GetCacheKey(string key) => $"{CacheItemPrefix}-{key}";

    #endregion
}
