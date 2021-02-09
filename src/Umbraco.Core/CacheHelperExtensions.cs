using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core
{

    /// <summary>
    /// Extension methods for the cache helper
    /// </summary>
    public static class CacheHelperExtensions
    {

        public const string PartialViewCacheKey = "Umbraco.Web.PartialViewCacheKey";

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
