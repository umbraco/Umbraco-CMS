using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// Implements a fast <see cref="IAppCache"/> on top of a concurrent dictionary.
    /// </summary>
    internal class FastDictionaryCacheProvider : IAppCache
    {
        /// <summary>
        /// Gets the internal items dictionary, for tests only!
        /// </summary>
        internal readonly ConcurrentDictionary<string, Lazy<object>> Items = new ConcurrentDictionary<string, Lazy<object>>();

        /// <inheritdoc />
        public object Get(string cacheKey)
        {
            Items.TryGetValue(cacheKey, out var result); // else null
            return result == null ? null : FastDictionaryAppCacheBase.GetSafeLazyValue(result); // return exceptions as null
        }

        /// <inheritdoc />
        public object Get(string cacheKey, Func<object> getCacheItem)
        {
            var result = Items.GetOrAdd(cacheKey, k => FastDictionaryAppCacheBase.GetSafeLazy(getCacheItem));

            var value = result.Value; // will not throw (safe lazy)
            if (!(value is FastDictionaryAppCacheBase.ExceptionHolder eh))
                return value;

            // and... it's in the cache anyway - so contrary to other cache providers,
            // which would trick with GetSafeLazyValue, we need to remove by ourselves,
            // in order NOT to cache exceptions

            Items.TryRemove(cacheKey, out result);
            eh.Exception.Throw(); // throw once!
            return null; // never reached
        }

        /// <inheritdoc />
        public IEnumerable<object> SearchByKey(string keyStartsWith)
        {
            return Items
                .Where(kvp => kvp.Key.InvariantStartsWith(keyStartsWith))
                .Select(kvp => FastDictionaryAppCacheBase.GetSafeLazyValue(kvp.Value))
                .Where(x => x != null);
        }

        /// <inheritdoc />
        public IEnumerable<object> SearchByRegex(string regex)
        {
            var compiled = new Regex(regex, RegexOptions.Compiled);
            return Items
                .Where(kvp => compiled.IsMatch(kvp.Key))
                .Select(kvp => FastDictionaryAppCacheBase.GetSafeLazyValue(kvp.Value))
                .Where(x => x != null);
        }

        /// <inheritdoc />
        public void Clear()
        {
            Items.Clear();
        }

        /// <inheritdoc />
        public void Clear(string key)
        {
            Items.TryRemove(key, out _);
        }

        /// <inheritdoc />
        public void ClearOfType(string typeName)
        {
            var type = TypeFinder.GetTypeByName(typeName);
            if (type == null) return;
            var isInterface = type.IsInterface;

            foreach (var kvp in Items
                .Where(x =>
                {
                    // entry.Value is Lazy<object> and not null, its value may be null
                    // remove null values as well, does not hurt
                    // get non-created as NonCreatedValue & exceptions as null
                    var value = FastDictionaryAppCacheBase.GetSafeLazyValue(x.Value, true);

                    // if T is an interface remove anything that implements that interface
                    // otherwise remove exact types (not inherited types)
                    return value == null || (isInterface ? (type.IsInstanceOfType(value)) : (value.GetType() == type));
                }))
                Items.TryRemove(kvp.Key, out _);
        }

        /// <inheritdoc />
        public void ClearOfType<T>()
        {
            var typeOfT = typeof(T);
            var isInterface = typeOfT.IsInterface;

            foreach (var kvp in Items
                .Where(x =>
                {
                    // entry.Value is Lazy<object> and not null, its value may be null
                    // remove null values as well, does not hurt
                    // compare on exact type, don't use "is"
                    // get non-created as NonCreatedValue & exceptions as null
                    var value = FastDictionaryAppCacheBase.GetSafeLazyValue(x.Value, true);

                    // if T is an interface remove anything that implements that interface
                    // otherwise remove exact types (not inherited types)
                    return value == null || (isInterface ? (value is T) : (value.GetType() == typeOfT));
                }))
                Items.TryRemove(kvp.Key, out _);
        }

        /// <inheritdoc />
        public void ClearOfType<T>(Func<string, T, bool> predicate)
        {
            var typeOfT = typeof(T);
            var isInterface = typeOfT.IsInterface;

            foreach (var kvp in Items
                .Where(x =>
                {
                    // entry.Value is Lazy<object> and not null, its value may be null
                    // remove null values as well, does not hurt
                    // compare on exact type, don't use "is"
                    // get non-created as NonCreatedValue & exceptions as null
                    var value = FastDictionaryAppCacheBase.GetSafeLazyValue(x.Value, true);
                    if (value == null) return true;

                    // if T is an interface remove anything that implements that interface
                    // otherwise remove exact types (not inherited types)
                    return (isInterface ? (value is T) : (value.GetType() == typeOfT))
                            // run predicate on the 'public key' part only, ie without prefix
                            && predicate(x.Key, (T)value);
                }))
                Items.TryRemove(kvp.Key, out _);
        }

        /// <inheritdoc />
        public void ClearByKey(string keyStartsWith)
        {
            foreach (var ikvp in Items
                .Where(kvp => kvp.Key.InvariantStartsWith(keyStartsWith)))
                Items.TryRemove(ikvp.Key, out _);
        }

        /// <inheritdoc />
        public void ClearByRegex(string regex)
        {
            var compiled = new Regex(regex, RegexOptions.Compiled);
            foreach (var ikvp in Items
                .Where(kvp => compiled.IsMatch(kvp.Key)))
                Items.TryRemove(ikvp.Key, out _);
        }
    }
}
