using System;
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
        protected internal static readonly object ValueNotCreated = new object();

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

        protected internal static Lazy<object> GetSafeLazy(Func<object> getCacheItem)
        {
            // try to generate the value and if it fails,
            // wrap in an ExceptionHolder - would be much simpler
            // to just use lazy.IsValueFaulted alas that field is
            // internal
            return new Lazy<object>(() =>
            {
                try
                {
                    return getCacheItem();
                }
                catch (Exception e)
                {
                    return new ExceptionHolder(e);
                }
            });
        }

        protected internal static object GetSafeLazyValue(Lazy<object> lazy, bool onlyIfValueIsCreated = false)
        {
            // if onlyIfValueIsCreated, do not trigger value creation
            // must return something, though, to differenciate from null values
            if (onlyIfValueIsCreated && lazy.IsValueCreated == false) return ValueNotCreated;

            // if execution has thrown then lazy.IsValueCreated is false
            // and lazy.IsValueFaulted is true (but internal) so we use our
            // own exception holder (see Lazy<T> source code) to return null
            if (lazy.Value is ExceptionHolder) return null;

            // we have a value and execution has not thrown so returning
            // here does not throw - unless we're re-entering, take care of it
            try
            {
                return lazy.Value;
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException("The method that computes a value for the cache has tried to read that value from the cache.", e);
            }
        }

        internal class ExceptionHolder
        {
            public ExceptionHolder(Exception e)
            {
                Exception = e;
            }

            public Exception Exception { get; private set; }
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
            var type = TypeFinder.GetTypeByName(typeName);
            if (type == null) return;
            var isInterface = type.IsInterface;
            using (WriteLock)
            {
                foreach (var entry in GetDictionaryEntries()
                    .Where(x =>
                    {
                        // entry.Value is Lazy<object> and not null, its value may be null
                        // remove null values as well, does not hurt
                        // get non-created as NonCreatedValue & exceptions as null
                        var value = GetSafeLazyValue((Lazy<object>)x.Value, true);

                        // if T is an interface remove anything that implements that interface
                        // otherwise remove exact types (not inherited types)
                        return value == null || (isInterface ? (type.IsInstanceOfType(value)) : (value.GetType() == type));
                    })
                    .ToArray())
                    RemoveEntry((string) entry.Key);
            }
        }

        public virtual void ClearCacheObjectTypes<T>()
        {
            var typeOfT = typeof(T);
            var isInterface = typeOfT.IsInterface;
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

                        // if T is an interface remove anything that implements that interface
                        // otherwise remove exact types (not inherited types)
                        return value == null || (isInterface ? (value is T) : (value.GetType() == typeOfT));
                    })
                    .ToArray())
                    RemoveEntry((string) entry.Key);
            }
        }

        public virtual void ClearCacheObjectTypes<T>(Func<string, T, bool> predicate)
        {
            var typeOfT = typeof(T);
            var isInterface = typeOfT.IsInterface;
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

                        // if T is an interface remove anything that implements that interface
                        // otherwise remove exact types (not inherited types)
                        return (isInterface ? (value is T) : (value.GetType() == typeOfT))
                               // run predicate on the 'public key' part only, ie without prefix
                               && predicate(((string) x.Key).Substring(plen), (T) value);
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