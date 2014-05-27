using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Umbraco.Core.Cache
{
    /// <summary>
    /// A cache provider that caches items in the HttpContext.Items
    /// </summary>
    internal class HttpRequestCacheProvider : DictionaryCacheProviderBase
    {
        private readonly Func<HttpContextBase> _context;

        public HttpRequestCacheProvider(HttpContext context)
        {
            // create wrapper only once!
            var wrapper = new HttpContextWrapper(context);
            _context = () => wrapper;
        }

        public HttpRequestCacheProvider(Func<HttpContextBase> context)
        {
            _context = context;
        }

        protected override IEnumerable<DictionaryEntry> GetDictionaryEntries()
        {
            const string prefix = CacheItemPrefix + "-";
            return _context().Items.Cast<DictionaryEntry>()
                .Where(x => x.Key is string && ((string)x.Key).StartsWith(prefix));
        }

        protected override void RemoveEntry(string key)
        {
            _context().Items.Remove(key);
        }

        protected override object GetEntry(string key)
        {
            return _context().Items[key];
        }

        #region Get

        public override object GetCacheItem(string cacheKey, Func<object> getCacheItem)
        {
            cacheKey = GetCacheKey(cacheKey);

            Lazy<object> result;

            using (var lck = new UpgradeableReadLock(Locker))
            {
                result = _context().Items[cacheKey] as Lazy<object>; // null if key not found
                if (result == null || (result.IsValueCreated && result.Value == null))
                {
                    lck.UpgradeToWriteLock();

                    result = new Lazy<object>(getCacheItem);
                    _context().Items[cacheKey] = result;
                }
            }

            return result.Value;
        }
        
        #endregion

        #region Insert
        #endregion

    }
}