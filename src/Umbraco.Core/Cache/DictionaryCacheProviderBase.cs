﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Umbraco.Core.Cache
{
    internal abstract class DictionaryCacheProviderBase : ICacheProvider
    {
        // prefix cache keys so we know which one are ours
        protected const string CacheItemPrefix = "umbrtmche";

        // an object that represent a value that has not been created yet
        protected readonly object ValueNotCreated = new object();

        // manupulate the underlying cache entries
        // these *must* be called from within the appropriate locks
        // and use the full prefixed cache keys
        protected abstract IEnumerable<DictionaryEntry> GetDictionaryEntries();
        protected abstract void RemoveEntry(string key);
        protected abstract object GetEntry(string key);

        // read-write lock the underlying cache
        protected abstract IDisposable ReadLock { get; }
        protected abstract IDisposable WriteLock { get; }

        protected string GetCacheKey(string key)
        {
            return string.Format("{0}-{1}", CacheItemPrefix, key);
        }

        protected object GetSafeLazyValue(Lazy<object> lazy, bool onlyIfValueIsCreated = false)
        {
            try
            {
                // if onlyIfValueIsCreated, do not trigger value creation
                // must return something, though, to differenciate from null values
                if (onlyIfValueIsCreated && lazy.IsValueCreated == false) return ValueNotCreated;
                return lazy.Value;
            }
            catch
            {
                return null;
            }
        }

        #region Clear

        public virtual void ClearAllCache()
        {
            using (WriteLock)
            {
                foreach (var entry in GetDictionaryEntries()
                    .ToArray())
                    RemoveEntry((string) entry.Key);
            }
        }

        public virtual void ClearCacheItem(string key)
        {
            var cacheKey = GetCacheKey(key);
            using (WriteLock)
            {
                RemoveEntry(cacheKey);
            }
        }

        public virtual void ClearCacheObjectTypes(string typeName)
        {
            using (WriteLock)
            {
                foreach (var entry in GetDictionaryEntries()
                    .Where(x =>
                    {
                        // entry.Value is Lazy<object> and not null, its value may be null
                        // remove null values as well, does not hurt
                        // get non-created as NonCreatedValue & exceptions as null
                        var value = GetSafeLazyValue((Lazy<object>)x.Value, true);
                        return value == null || value.GetType().ToString().InvariantEquals(typeName);
                    })
                    .ToArray())
                    RemoveEntry((string) entry.Key);
            }
        }

        public virtual void ClearCacheObjectTypes<T>()
        {
            var typeOfT = typeof(T);
            using (WriteLock)
            {
                foreach (var entry in GetDictionaryEntries()
                    .Where(x =>
                    {
                        // entry.Value is Lazy<object> and not null, its value may be null
                        // remove null values as well, does not hurt
                        // compare on exact type, don't use "is"
                        // get non-created as NonCreatedValue & exceptions as null
                        var value = GetSafeLazyValue((Lazy<object>)x.Value, true);
                        return value == null || value.GetType() == typeOfT;
                    })
                    .ToArray())
                    RemoveEntry((string) entry.Key);
            }
        }

        public virtual void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
            var typeOfT = typeof(T);
            var plen = CacheItemPrefix.Length + 1;
            using (WriteLock)
            {
                foreach (var entry in GetDictionaryEntries()
                    .Where(x =>
                    {
                        // entry.Value is Lazy<object> and not null, its value may be null
                        // remove null values as well, does not hurt
                        // compare on exact type, don't use "is"
                        // get non-created as NonCreatedValue & exceptions as null
                        var value = GetSafeLazyValue((Lazy<object>)x.Value, true);
                        if (value == null) return true;
                        return value.GetType() == typeOfT
                            // run predicate on the 'public key' part only, ie without prefix
                            && predicate(((string)x.Key).Substring(plen), (T)value);
                    }))
                    RemoveEntry((string) entry.Key);
            }
        }

        public virtual void ClearCacheByKeySearch(string keyStartsWith)
        {
            var plen = CacheItemPrefix.Length + 1;
            using (WriteLock)
            {
                foreach (var entry in GetDictionaryEntries()
                    .Where(x => ((string)x.Key).Substring(plen).InvariantStartsWith(keyStartsWith))
                    .ToArray())
                    RemoveEntry((string) entry.Key);
            }
        }

        public virtual void ClearCacheByKeyExpression(string regexString)
        {
            var plen = CacheItemPrefix.Length + 1;
            using (WriteLock)
            {
                foreach (var entry in GetDictionaryEntries()
                    .Where(x => Regex.IsMatch(((string)x.Key).Substring(plen), regexString))
                    .ToArray())
                    RemoveEntry((string) entry.Key);
            }
        }

        #endregion

        #region Get

        public virtual IEnumerable<object> GetCacheItemsByKeySearch(string keyStartsWith)
        {
            var plen = CacheItemPrefix.Length + 1;
            IEnumerable<DictionaryEntry> entries;
            using (ReadLock)
            {
                entries = GetDictionaryEntries()
                    .Where(x => ((string)x.Key).Substring(plen).InvariantStartsWith(keyStartsWith))
                    .ToArray(); // evaluate while locked
            }
            return entries
                    .Select(x => GetSafeLazyValue((Lazy<object>)x.Value)) // return exceptions as null
                    .Where(x => x != null); // backward compat, don't store null values in the cache
        }

        public virtual IEnumerable<object> GetCacheItemsByKeyExpression(string regexString)
        {
            const string prefix = CacheItemPrefix + "-";
            var plen = prefix.Length;
            IEnumerable<DictionaryEntry> entries;
            using (ReadLock)
            {
                entries = GetDictionaryEntries()
                    .Where(x => Regex.IsMatch(((string)x.Key).Substring(plen), regexString))
                    .ToArray(); // evaluate while locked
            }
            return entries
                    .Select(x => GetSafeLazyValue((Lazy<object>)x.Value)) // return exceptions as null
                    .Where(x => x != null); // backward compat, don't store null values in the cache
        }

        public virtual object GetCacheItem(string cacheKey)
        {
            cacheKey = GetCacheKey(cacheKey);
            Lazy<object> result;
            using (ReadLock)
            {
                result = GetEntry(cacheKey) as Lazy<object>; // null if key not found
            }
            return result == null ? null : GetSafeLazyValue(result); // return exceptions as null
        }

        public abstract object GetCacheItem(string cacheKey, Func<object> getCacheItem);

        #endregion
    }
}