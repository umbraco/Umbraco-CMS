using System;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Umbraco.Core.Cache;

namespace Umbraco.Web
{

    /// <summary>
    /// Extension methods for the cache helper
    /// </summary>
    public static class CacheHelperExtensions
    {

        public const string PartialViewCacheKey = "Umbraco.Web.PartialViewCacheKey";

        /// <summary>
        /// Outputs and caches a partial view in MVC
        /// </summary>
        /// <param name="appCaches"></param>
        /// <param name="htmlHelper"></param>
        /// <param name="partialViewName"></param>
        /// <param name="model"></param>
        /// <param name="cachedSeconds"></param>
        /// <param name="cacheKey">used to cache the partial view, this key could change if it is cached by page or by member</param>
        /// <param name="viewData"></param>
        /// <returns></returns>
        public static IHtmlString CachedPartialView(
            this AppCaches appCaches,
            HtmlHelper htmlHelper,
            string partialViewName,
            object model,
            int cachedSeconds,
            string cacheKey,
            ViewDataDictionary viewData = null)
        {
            //disable cached partials in debug mode: http://issues.umbraco.org/issue/U4-5940
            //disable cached partials in preview mode: https://github.com/umbraco/Umbraco-CMS/issues/10384
            if (htmlHelper.ViewContext.HttpContext.IsDebuggingEnabled || Umbraco.Web.Composing.Current.UmbracoContext.InPreviewMode)
            {
                // just return a normal partial view instead
                return htmlHelper.Partial(partialViewName, model, viewData);
            }

            return appCaches.RuntimeCache.GetCacheItem<IHtmlString>(
                PartialViewCacheKey + cacheKey,
                () => htmlHelper.Partial(partialViewName, model, viewData),
                priority: CacheItemPriority.NotRemovable, //not removable, the same as macros (apparently issue #27610)
                timeout: new TimeSpan(0, 0, 0, cachedSeconds));
        }

        /// <summary>
        /// Clears the cache for partial views
        /// </summary>
        /// <param name="appCaches"></param>
        public static void ClearPartialViewCache(this AppCaches appCaches)
        {
            appCaches.RuntimeCache.ClearByKey(PartialViewCacheKey);
        }
    }
}
