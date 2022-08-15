using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Extensions;

/// <summary>
///     Extension methods for the cache helper
/// </summary>
public static class CacheHelperExtensions
{
    /// <summary>
    ///     Outputs and caches a partial view in MVC
    /// </summary>
    /// <param name="appCaches"></param>
    /// <param name="hostingEnvironment"></param>
    /// <param name="umbracoContext"></param>
    /// <param name="htmlHelper"></param>
    /// <param name="partialViewName"></param>
    /// <param name="model"></param>
    /// <param name="cacheTimeout"></param>
    /// <param name="cacheKey">used to cache the partial view, this key could change if it is cached by page or by member</param>
    /// <param name="viewData"></param>
    /// <returns></returns>
    public static IHtmlContent? CachedPartialView(
        this AppCaches appCaches,
        IHostingEnvironment hostingEnvironment,
        IUmbracoContext umbracoContext,
        IHtmlHelper htmlHelper,
        string partialViewName,
        object model,
        TimeSpan cacheTimeout,
        string cacheKey,
        ViewDataDictionary? viewData = null)
    {
        // disable cached partials in debug mode: http://issues.umbraco.org/issue/U4-5940
        // disable cached partials in preview mode: https://github.com/umbraco/Umbraco-CMS/issues/10384
        if (hostingEnvironment.IsDebugMode || umbracoContext.InPreviewMode)
        {
            // just return a normal partial view instead
            return htmlHelper.Partial(partialViewName, model, viewData);
        }

        IHtmlContent? result = appCaches.RuntimeCache.GetCacheItem<IHtmlContent>(
            CoreCacheHelperExtensions.PartialViewCacheKey + cacheKey,
            () => new HtmlString(htmlHelper.Partial(partialViewName, model, viewData).ToHtmlString()),
            cacheTimeout);

        return result;
    }
}
