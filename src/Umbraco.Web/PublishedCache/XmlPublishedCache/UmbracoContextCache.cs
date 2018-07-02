using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    static class UmbracoContextCache
    {
        static readonly ConditionalWeakTable<UmbracoContext, ConcurrentDictionary<string, object>> Caches
            = new ConditionalWeakTable<UmbracoContext, ConcurrentDictionary<string, object>>();

        public static ConcurrentDictionary<string, object> Current
        {
            get
            {
                var umbracoContext = UmbracoContext.Current;

                // will get or create a value
                // a ConditionalWeakTable is thread-safe
                // does not prevent the context from being disposed, and then the dictionary will be disposed too
                return umbracoContext == null ? null : Caches.GetOrCreateValue(umbracoContext);
            }
        }
    }
}
