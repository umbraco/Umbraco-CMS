using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Umbraco.Web;

namespace Umbraco.Tests.LegacyXmlPublishedCache
{
    static class UmbracoContextCache
    {
        static readonly ConditionalWeakTable<UmbracoContext, ConcurrentDictionary<string, object>> Caches
            = new ConditionalWeakTable<UmbracoContext, ConcurrentDictionary<string, object>>();

        public static ConcurrentDictionary<string, object> Current
        {
            get
            {
                var umbracoContext = Umbraco.Web.Composing.Current.UmbracoContext;

                // will get or create a value
                // a ConditionalWeakTable is thread-safe
                // does not prevent the context from being disposed, and then the dictionary will be disposed too
                return umbracoContext == null ? null : Caches.GetOrCreateValue(umbracoContext);
            }
        }
    }
}
