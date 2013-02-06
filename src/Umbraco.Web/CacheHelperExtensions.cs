using System;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Cache;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.cache;

namespace Umbraco.Web
{

	/// <summary>
	/// Extension methods for the cache helper
	/// </summary>
	internal static class CacheHelperExtensions
	{
		/// <summary>
		/// Application event handler to bind to events to clear the cache for the cache helper extensions
		/// </summary>		
		internal sealed class CacheHelperApplicationEventListener : ApplicationEventHandler
		{
            protected override void ApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
			{
                if (applicationContext != null)
				{
					//bind to events to clear the cache, after publish, after media save and after member save

					Document.AfterPublish
						+= (sender, args) =>
                           applicationContext.ApplicationCache.ClearPartialViewCache();

					global::umbraco.cms.businesslogic.media.Media.AfterSave
						+= (sender, args) =>
                           applicationContext.ApplicationCache.ClearPartialViewCache();

					global::umbraco.cms.businesslogic.member.Member.AfterSave
						+= (sender, args) =>
                           applicationContext.ApplicationCache.ClearPartialViewCache();
				}
			}

		}

		public const string PartialViewCacheKey = "Umbraco.Web.PartialViewCacheKey";
        private const string TemplateCacheKey = "template";

	    /// <summary>
	    /// Clears the cache for a template
	    /// </summary>
        /// <param name="cacheHelper"></param>
	    /// <param name="templateId"></param>
        public static void ClearCacheForTemplate(this CacheHelper cacheHelper, int templateId)
        {
            cacheHelper.ClearCacheByKeySearch(
                string.Format("{0}{1}", TemplateCacheKey, templateId));            
        }

	    /// <summary>
	    /// Clears the library cache for media
	    /// </summary>
	    /// <param name="cacheHelper"></param>
	    /// <param name="mediaId"></param>
	    /// <param name="allServers">
	    /// If set to false, this will only clear the library cache for the current server, not all servers registered in the 
	    /// server farm. In most cases if you are clearing cache you would probably clear it on all servers.
	    /// </param>
	    public static void ClearLibraryCacheForMedia(this CacheHelper cacheHelper, int mediaId, bool allServers = true)
        {
            const string getmediaCacheKey = "GetMedia";

            if (allServers && UmbracoSettings.UseDistributedCalls)
            {
                DistributedCache.Instance.RefreshMediaCache(mediaId);
            }
            else
            {
                var m = new global::umbraco.cms.businesslogic.media.Media(mediaId);
                if (m.nodeObjectType == global::umbraco.cms.businesslogic.media.Media._objectType)
                {
                    foreach (string id in m.Path.Split(','))
                    {
                        cacheHelper.ClearCacheByKeySearch(
                            string.Format("UL_{0}_{1}_True", getmediaCacheKey, id));

                        // Also clear calls that only query this specific item!
                        if (id == m.Id.ToString())
                            cacheHelper.ClearCacheByKeySearch(
                                string.Format("UL_{0}_{1}", getmediaCacheKey, id));

                    }
                }
            }
        }

		/// <summary>
		/// Outputs and caches a partial view in MVC
		/// </summary>
		/// <param name="cacheHelper"></param>
		/// <param name="htmlHelper"></param>
		/// <param name="partialViewName"></param>
		/// <param name="model"></param>
		/// <param name="cachedSeconds"></param>
		/// <param name="cacheKey">used to cache the partial view, this key could change if it is cached by page or by member</param>
		/// <param name="viewData"></param>
		/// <returns></returns>
		public static IHtmlString CachedPartialView(
			this CacheHelper cacheHelper,
			HtmlHelper htmlHelper,
			string partialViewName,
			object model,
			int cachedSeconds,
			string cacheKey,
			ViewDataDictionary viewData = null)
		{
			return cacheHelper.GetCacheItem(
				PartialViewCacheKey + cacheKey,
				CacheItemPriority.NotRemovable, //not removable, the same as macros (apparently issue #27610)
				null,
				new TimeSpan(0, 0, 0, cachedSeconds),
				() => htmlHelper.Partial(partialViewName, model, viewData));
		}

		/// <summary>
		/// Clears the cache for partial views
		/// </summary>
		/// <param name="cacheHelper"></param>
		public static void ClearPartialViewCache(this CacheHelper cacheHelper)
		{
			cacheHelper.ClearCacheByKeySearch(PartialViewCacheKey);
		}
	}
}