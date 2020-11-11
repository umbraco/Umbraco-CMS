using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Umbraco.Core.Cache;
using Umbraco.Core.Hosting;

namespace Umbraco.Extensions
{

    /// <summary>
    /// Extension methods for the cache helper
    /// </summary>
    public static class CacheHelperExtensions
    {
        /// <summary>
        /// Outputs and caches a partial view in MVC
        /// </summary>
        /// <param name="appCaches"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="htmlHelper"></param>
        /// <param name="partialViewName"></param>
        /// <param name="model"></param>
        /// <param name="cachedSeconds"></param>
        /// <param name="cacheKey">used to cache the partial view, this key could change if it is cached by page or by member</param>
        /// <param name="viewData"></param>
        /// <returns></returns>
        public static IHtmlContent CachedPartialView(
            this AppCaches appCaches,
            IHostingEnvironment hostingEnvironment,
            IHtmlHelper htmlHelper,
            string partialViewName,
            object model,
            int cachedSeconds,
            string cacheKey,
            ViewDataDictionary viewData = null)
        {
            //disable cached partials in debug mode: http://issues.umbraco.org/issue/U4-5940
            if (hostingEnvironment.IsDebugMode)
            {
                // just return a normal partial view instead
                return htmlHelper.Partial(partialViewName, model, viewData);
            }

            return appCaches.RuntimeCache.GetCacheItem<IHtmlContent>(
                Core.CacheHelperExtensions.PartialViewCacheKey + cacheKey,
                () => htmlHelper.Partial(partialViewName, model, viewData),
                timeout: new TimeSpan(0, 0, 0, cachedSeconds));
        }

    }
}
