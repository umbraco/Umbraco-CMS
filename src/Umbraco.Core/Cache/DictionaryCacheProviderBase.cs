using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Umbraco.Core.Cache
{
    internal abstract class DictionaryCacheProviderBase : ICacheProvider
    {
        // prefix cache keys so we know which one are ours
        protected const string CacheItemPrefix = "umbrtmche";

        protected readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        // manupulate the underlying cache entries
        // these *must* be called from within the appropriate locks
        // and use the full prefixed cache keys
        protected abstract IEnumerable<DictionaryEntry> GetDictionaryEntries();
        protected abstract void RemoveEntry(string key);
        protected abstract object GetEntry(string key);

        protected string GetCacheKey(string key)
        {
            return string.Format("{0}-{1}", CacheItemPrefix, key);
        }

        protected object GetSafeLazyValue(Lazy<object> lazy)
        {
            try
            {
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
            using (new WriteLock(Locker))
            {
                foreach (var entry in GetDictionaryEntries()
                    .ToArray())
                    RemoveEntry((string) entry.Key);
            }
        }

        public virtual void ClearCacheItem(string key)
        {
            using (new WriteLock(Locker))
            {
                var cacheKey = GetCacheKey(key);
                RemoveEntry(cacheKey);
            }
        }

        public virtual void ClearCacheObjectTypes(string typeName)
        {
            using (new WriteLock(Locker))
            {
                foreach (var entry in GetDictionaryEntries()
                    .Where(x =>
                    {
                        // entry.Value is Lazy<object> and not null, its value may be null
                        // remove null values as well, does not hurt
                        var value = GetSafeLazyValue((Lazy<object>) x.Value); // return exceptions as null
                        return value == null || value.GetType().ToString().InvariantEquals(typeName);
                    })
                    .ToArray())
                    RemoveEntry((string) entry.Key);
            }
        }

        public virtual void ClearCacheObjectTypes<T>()
        {
            var typeOfT = typeof(T);
            using (new WriteLock(Locker))
            {
                foreach (var entry in GetDictionaryEntries()
                    .Where(x =>
                    {
                        // entry.Value is Lazy<object> and not null, its value may be null
                        // remove null values as well, does not hurt
                        // compare on exact type, don't use "is"
                        var value = GetSafeLazyValue((Lazy<object>)x.Value); // return exceptions as null
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
            using (new WriteLock(Locker))
            {
                foreach (var entry in GetDictionaryEntries()
                    .Where(x =>
                    {
                        // entry.Value is Lazy<object> and not null, its value may be null
                        // remove null values as well, does not hurt
                        // compare on exact type, don't use "is"
                        var value = GetSafeLazyValue((Lazy<object>)x.Value); // return exceptions as null
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
            using (new WriteLock(Locker))
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
            using (new WriteLock(Locker))
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
            using (new ReadLock(Locker))
            {
                return GetDictionaryEntries()
                    .Where(x => ((string)x.Key).Substring(plen).InvariantStartsWith(keyStartsWith))
                    .Select(x => GetSafeLazyValue((Lazy<object>)x.Value)) // return exceptions as null
                    .Where(x => x != null) // backward compat, don't store null values in the cache
                    .ToList();
            }
        }

        public virtual IEnumerable<object> GetCacheItemsByKeyExpression(string regexString)
        {
            const string prefix = CacheItemPrefix + "-";
            var plen = prefix.Length;
            using (new ReadLock(Locker))
            {
                return GetDictionaryEntries()
                    .Where(x => Regex.IsMatch(((string)x.Key).Substring(plen), regexString))
                    .Select(x => GetSafeLazyValue((Lazy<object>)x.Value)) // return exceptions as null
                    .Where(x => x != null) // backward compat, don't store null values in the cache
                    .ToList();
            }
        }

        public virtual object GetCacheItem(string cacheKey)
        {
            cacheKey = GetCacheKey(cacheKey);
            using (new ReadLock(Locker))
            {
                var result = GetEntry(cacheKey) as Lazy<object>; // null if key not found
                return result == null ? null : GetSafeLazyValue(result); // return exceptions as null
            }
        }

        public abstract object GetCacheItem(string cacheKey, Func<object> getCacheItem);

        #endregion
    }
}