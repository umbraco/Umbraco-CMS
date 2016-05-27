using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Plugins;

namespace Umbraco.Core.Cache
{
    class DictionaryCacheProvider : ICacheProvider
    {
        private readonly ConcurrentDictionary<string, Lazy<object>> _items
            = new ConcurrentDictionary<string, Lazy<object>>();

        public void ClearAllCache()
        {
            _items.Clear();
        }

        public void ClearCacheItem(string key)
        {
            Lazy<object> item;
            _items.TryRemove(key, out item);
        }

        public void ClearCacheObjectTypes(string typeName)
        {
            var type = TypeFinder.GetTypeByName(typeName);
            if (type == null) return;
            var isInterface = type.IsInterface;

            Lazy<object> item;
            foreach (var kvp in _items
                .Where(x =>
                {
                    // entry.Value is Lazy<object> and not null, its value may be null
                    // remove null values as well, does not hurt
                    // get non-created as NonCreatedValue & exceptions as null
                    var value = DictionaryCacheProviderBase.GetSafeLazyValue(x.Value, true);

                    // if T is an interface remove anything that implements that interface
                    // otherwise remove exact types (not inherited types)
                    return value == null || (isInterface ? (type.IsInstanceOfType(value)) : (value.GetType() == type));
                }))
                _items.TryRemove(kvp.Key, out item);
        }

        public void ClearCacheObjectTypes<T>()
        {
            var typeOfT = typeof(T);
            var isInterface = typeOfT.IsInterface;

            Lazy<object> item;
            foreach (var kvp in _items
                .Where(x =>
                {
                    // entry.Value is Lazy<object> and not null, its value may be null
                    // remove null values as well, does not hurt
                    // compare on exact type, don't use "is"
                    // get non-created as NonCreatedValue & exceptions as null
                    var value = DictionaryCacheProviderBase.GetSafeLazyValue(x.Value, true);

                    // if T is an interface remove anything that implements that interface
                    // otherwise remove exact types (not inherited types)
                    return value == null || (isInterface ? (value is T) : (value.GetType() == typeOfT));
                }))
                _items.TryRemove(kvp.Key, out item);
        }

        public void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
            var typeOfT = typeof(T);
            var isInterface = typeOfT.IsInterface;

            Lazy<object> item;
            foreach (var kvp in _items
                .Where(x =>
                {
                    // entry.Value is Lazy<object> and not null, its value may be null
                    // remove null values as well, does not hurt
                    // compare on exact type, don't use "is"
                    // get non-created as NonCreatedValue & exceptions as null
                    var value = DictionaryCacheProviderBase.GetSafeLazyValue(x.Value, true);
                    if (value == null) return true;

                    // if T is an interface remove anything that implements that interface
                    // otherwise remove exact types (not inherited types)
                    return (isInterface ? (value is T) : (value.GetType() == typeOfT))
                            // run predicate on the 'public key' part only, ie without prefix
                            && predicate(x.Key, (T)value);
                }))
                _items.TryRemove(kvp.Key, out item);
        }

        public void ClearCacheByKeySearch(string keyStartsWith)
        {
            Lazy<object> item;
            foreach (var ikvp in _items
                .Where(kvp => kvp.Key.InvariantStartsWith(keyStartsWith)))
                _items.TryRemove(ikvp.Key, out item);
        }

        public void ClearCacheByKeyExpression(string regexString)
        {
            Lazy<object> item;
            foreach (var ikvp in _items
                .Where(kvp => Regex.IsMatch(kvp.Key, regexString)))
                _items.TryRemove(ikvp.Key, out item);
        }

        public IEnumerable<object> GetCacheItemsByKeySearch(string keyStartsWith)
        {
            return _items
                .Where(kvp => kvp.Key.InvariantStartsWith(keyStartsWith))
                .Select(kvp => DictionaryCacheProviderBase.GetSafeLazyValue(kvp.Value))
                .Where(x => x != null);
        }

        public IEnumerable<object> GetCacheItemsByKeyExpression(string regexString)
        {
            return _items
                .Where(kvp => Regex.IsMatch(kvp.Key, regexString))
                .Select(kvp => DictionaryCacheProviderBase.GetSafeLazyValue(kvp.Value))
                .Where(x => x != null);
        }

        public object GetCacheItem(string cacheKey)
        {
            Lazy<object> result;
            _items.TryGetValue(cacheKey, out result); // else null
            return result == null ? null : DictionaryCacheProviderBase.GetSafeLazyValue(result); // return exceptions as null
        }

        public object GetCacheItem(string cacheKey, Func<object> getCacheItem)
        {
            var result = _items.GetOrAdd(cacheKey, k => DictionaryCacheProviderBase.GetSafeLazy(getCacheItem));

            var value = result.Value; // will not throw (safe lazy)
            var eh = value as DictionaryCacheProviderBase.ExceptionHolder;
            if (eh == null)
                return value;

            // and... it's in the cache anyway - so contrary to other cache providers,
            // which would trick with GetSafeLazyValue, we need to remove by ourselves,
            // in order NOT to cache exceptions

            _items.TryRemove(cacheKey, out result);
            throw eh.Exception; // throw once!
        }
    }
}
