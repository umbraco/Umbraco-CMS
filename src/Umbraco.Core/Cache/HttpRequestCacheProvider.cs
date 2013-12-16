using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
            _context = () => new HttpContextWrapper(context);
        }

        public HttpRequestCacheProvider(Func<HttpContextBase> context)
        {
            _context = context;
        }

        protected override DictionaryCacheWrapper DictionaryCache
        {
            get
            {
                var ctx = _context();
                return new DictionaryCacheWrapper(
                    ctx.Items,
                    o => ctx.Items[o],
                    o => ctx.Items.Remove(o));
            }
        }

        public override object GetCacheItem(string cacheKey, Func<object> getCacheItem)
        {
            var ctx = _context();
            var ck = GetCacheKey(cacheKey);
            return ctx.Items[ck] ?? (ctx.Items[ck] = getCacheItem());
        }

    }
}