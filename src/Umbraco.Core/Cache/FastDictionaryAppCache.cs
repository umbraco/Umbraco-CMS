﻿using System;
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
    public class FastDictionaryAppCache : IAppCache
    {
        private readonly ITypeFinder _typeFinder;

        public FastDictionaryAppCache(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder ?? throw new ArgumentNullException(nameof(typeFinder));
        }

        /// <summary>
        /// Gets the internal items dictionary, for tests only!
        /// </summary>
        private readonly ConcurrentDictionary<string, Lazy<object>> _items = new ConcurrentDictionary<string, Lazy<object>>();

        public int Count => _items.Count;

        /// <inheritdoc />
        public object Get(string cacheKey)
        {
            _items.TryGetValue(cacheKey, out var result); // else null
            return result == null ? null : SafeLazy.GetSafeLazyValue(result); // return exceptions as null
        }

        /// <inheritdoc />
        public object Get(string cacheKey, Func<object> getCacheItem)
        {
            var result = _items.GetOrAdd(cacheKey, k => SafeLazy.GetSafeLazy(getCacheItem));

            var value = result.Value; // will not throw (safe lazy)
            if (!(value is SafeLazy.ExceptionHolder eh))
                return value;

            // and... it's in the cache anyway - so contrary to other cache providers,
            // which would trick with GetSafeLazyValue, we need to remove by ourselves,
            // in order NOT to cache exceptions

            _items.TryRemove(cacheKey, out result);
            eh.Exception.Throw(); // throw once!
            return null; // never reached
        }

        /// <inheritdoc />
        public IEnumerable<object> SearchByKey(string keyStartsWith)
        {
            return _items
                .Where(kvp => kvp.Key.InvariantStartsWith(keyStartsWith))
                .Select(kvp => SafeLazy.GetSafeLazyValue(kvp.Value))
                .Where(x => x != null);
        }

        /// <inheritdoc />
        public IEnumerable<object> SearchByRegex(string regex)
        {
            var compiled = new Regex(regex, RegexOptions.Compiled);
            return _items
                .Where(kvp => compiled.IsMatch(kvp.Key))
                .Select(kvp => SafeLazy.GetSafeLazyValue(kvp.Value))
                .Where(x => x != null);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _items.Clear();
        }

        /// <inheritdoc />
        public void Clear(string key)
        {
            _items.TryRemove(key, out _);
        }

        /// <inheritdoc />
        public void ClearOfType(string typeName)
        {
            var type = _typeFinder.GetTypeByName(typeName);
            if (type == null) return;
            var isInterface = type.IsInterface;

            foreach (var kvp in _items
                .Where(x =>
                {
                    // entry.Value is Lazy<object> and not null, its value may be null
                    // remove null values as well, does not hurt
                    // get non-created as NonCreatedValue & exceptions as null
                    var value = SafeLazy.GetSafeLazyValue(x.Value, true);

                    // if T is an interface remove anything that implements that interface
                    // otherwise remove exact types (not inherited types)
                    return value == null || (isInterface ? (type.IsInstanceOfType(value)) : (value.GetType() == type));
                }))
                _items.TryRemove(kvp.Key, out _);
        }

        /// <inheritdoc />
        public void ClearOfType<T>()
        {
            var typeOfT = typeof(T);
            var isInterface = typeOfT.IsInterface;

            foreach (var kvp in _items
                .Where(x =>
                {
                    // entry.Value is Lazy<object> and not null, its value may be null
                    // remove null values as well, does not hurt
                    // compare on exact type, don't use "is"
                    // get non-created as NonCreatedValue & exceptions as null
                    var value = SafeLazy.GetSafeLazyValue(x.Value, true);

                    // if T is an interface remove anything that implements that interface
                    // otherwise remove exact types (not inherited types)
                    return value == null || (isInterface ? (value is T) : (value.GetType() == typeOfT));
                }))
                _items.TryRemove(kvp.Key, out _);
        }

        /// <inheritdoc />
        public void ClearOfType<T>(Func<string, T, bool> predicate)
        {
            var typeOfT = typeof(T);
            var isInterface = typeOfT.IsInterface;

            foreach (var kvp in _items
                .Where(x =>
                {
                    // entry.Value is Lazy<object> and not null, its value may be null
                    // remove null values as well, does not hurt
                    // compare on exact type, don't use "is"
                    // get non-created as NonCreatedValue & exceptions as null
                    var value = SafeLazy.GetSafeLazyValue(x.Value, true);
                    if (value == null) return true;

                    // if T is an interface remove anything that implements that interface
                    // otherwise remove exact types (not inherited types)
                    return (isInterface ? (value is T) : (value.GetType() == typeOfT))
                            // run predicate on the 'public key' part only, ie without prefix
                            && predicate(x.Key, (T)value);
                }))
                _items.TryRemove(kvp.Key, out _);
        }

        /// <inheritdoc />
        public void ClearByKey(string keyStartsWith)
        {
            foreach (var ikvp in _items
                .Where(kvp => kvp.Key.InvariantStartsWith(keyStartsWith)))
                _items.TryRemove(ikvp.Key, out _);
        }

        /// <inheritdoc />
        public void ClearByRegex(string regex)
        {
            var compiled = new Regex(regex, RegexOptions.Compiled);
            foreach (var ikvp in _items
                .Where(kvp => compiled.IsMatch(kvp.Key)))
                _items.TryRemove(ikvp.Key, out _);
        }
    }
}
