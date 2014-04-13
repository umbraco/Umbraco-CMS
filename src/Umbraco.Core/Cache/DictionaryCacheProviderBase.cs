using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Umbraco.Core.Cache
{
    internal abstract class DictionaryCacheProviderBase : ICacheProvider
    {
        protected static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        protected abstract DictionaryCacheWrapper DictionaryCache { get; }

        /// <summary>
        /// Clears everything in umbraco's runtime cache
        /// </summary>
        /// <remarks>
        /// Does not clear other stuff the user has put in httpruntime.cache!
        /// </remarks>
        public virtual void ClearAllCache()
        {
            using (new WriteLock(Locker))
            {
                var keysToRemove = DictionaryCache.Cast<object>()
                                                  .Select(item => new DictionaryItemWrapper(item))
                                                  .Where(c => c.Key is string && ((string)c.Key).StartsWith(CacheItemPrefix) && DictionaryCache[c.Key.ToString()] != null)
                                                  .Select(c => c.Key)
                                                  .ToList();

                foreach (var k in keysToRemove)
                {
                    DictionaryCache.Remove(k);
                }
            }
        }

        /// <summary>
        /// Clears the item in umbraco's runtime cache with the given key 
        /// </summary>
        /// <param name="key">Key</param>
        public virtual void ClearCacheItem(string key)
        {
            using (new WriteLock(Locker))
            {
                if (DictionaryCache[GetCacheKey(key)] == null) return;
                DictionaryCache.Remove(GetCacheKey(key)); ;
            }
        }

        /// <summary>
        /// Clears all objects in the System.Web.Cache with the System.Type name as the
        /// input parameter. (using [object].GetType())
        /// </summary>
        /// <param name="typeName">The name of the System.Type which should be cleared from cache ex "System.Xml.XmlDocument"</param>
        public virtual void ClearCacheObjectTypes(string typeName)
        {
            using (new WriteLock(Locker))
            {
                var keysToRemove = DictionaryCache
                    .Cast<object>()
                    .Select(item => new DictionaryItemWrapper(item))
                    .Where(c =>
                    {
                        var k = c.Key.ToString();
                        var v = DictionaryCache[k];
                        return v != null && v.GetType().ToString().InvariantEquals(typeName);
                    })
                    .Select(c => c.Key)
                    .ToList();

                foreach (var k in keysToRemove)
                    DictionaryCache.Remove(k);
            }
        }

        public virtual void ClearCacheObjectTypes<T>()
        {
            using (new WriteLock(Locker))
            {
                var typeOfT = typeof(T);
                var keysToRemove = DictionaryCache
                    .Cast<object>()
                    .Select(item => new DictionaryItemWrapper(item))
                    .Where(c =>
                    {
                        var k = c.Key.ToString();
                        var v = DictionaryCache[k];
                        return v != null && v.GetType() == typeOfT;
                    })
                    .Select(c => c.Key)
                    .ToList();

                foreach (var k in keysToRemove)
                    DictionaryCache.Remove(k);
            }
        }

        public virtual void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
            using (new WriteLock(Locker))
            {
                var typeOfT = typeof(T);
                var keysToRemove = DictionaryCache
                    .Cast<object>()
                    .Select(item => new DictionaryItemWrapper(item))
                    .Where(c =>
                    {
                        var k = c.Key.ToString();
                        var v = DictionaryCache[k];
                        return v != null && v.GetType() == typeOfT && predicate(k, (T)v);
                    })
                    .Select(c => c.Key)
                    .ToList();

                foreach (var k in keysToRemove)
                    DictionaryCache.Remove(k);
            }
        }

        /// <summary>
        /// Clears all cache items that starts with the key passed.
        /// </summary>
        /// <param name="keyStartsWith">The start of the key</param>
        public virtual void ClearCacheByKeySearch(string keyStartsWith)
        {
            var keysToRemove = DictionaryCache.Cast<object>()
                                              .Select(item => new DictionaryItemWrapper(item))
                                              .Where(c => c.Key is string && ((string)c.Key).InvariantStartsWith(string.Format("{0}-{1}", CacheItemPrefix, keyStartsWith)))
                                              .Select(c => c.Key)
                                              .ToList();

            foreach (var k in keysToRemove)
            {
                DictionaryCache.Remove(k);
            }
        }

        /// <summary>
        /// Clears all cache items that have a key that matches the regular expression
        /// </summary>
        /// <param name="regexString"></param>
        public virtual void ClearCacheByKeyExpression(string regexString)
        {
            var keysToRemove = new List<object>();
            foreach (var item in DictionaryCache)
            {
                var c = new DictionaryItemWrapper(item);
                var s = c.Key as string;
                if (s != null)
                {
                    var withoutPrefix = s.TrimStart(string.Format("{0}-", CacheItemPrefix));
                    if (Regex.IsMatch(withoutPrefix, regexString))
                    {
                        keysToRemove.Add(c.Key);
                    }
                }
            }

            foreach (var k in keysToRemove)
            {
                DictionaryCache.Remove(k);
            }
        }

        public virtual IEnumerable<object> GetCacheItemsByKeySearch(string keyStartsWith)
        {
            return (from object item in DictionaryCache
                    select new DictionaryItemWrapper(item)
                    into c
                    where c.Key is string && ((string) c.Key).InvariantStartsWith(string.Format("{0}-{1}", CacheItemPrefix, keyStartsWith))
                    select c.Value).ToList();
        }

        /// <summary>
        /// Returns a cache item by key, does not update the cache if it isn't there.
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public virtual object GetCacheItem(string cacheKey)
        {
            var result = DictionaryCache.Get(GetCacheKey(cacheKey));
            return result;
        }

        public abstract object GetCacheItem(string cacheKey, Func<object> getCacheItem);        

        /// <summary>
        /// We prefix all cache keys with this so that we know which ones this class has created when 
        /// using the HttpRuntime cache so that when we clear it we don't clear other entries we didn't create.
        /// </summary>
        protected const string CacheItemPrefix = "umbrtmche";

        protected string GetCacheKey(string key)
        {
            return string.Format("{0}-{1}", CacheItemPrefix, key);
        }
    }
}