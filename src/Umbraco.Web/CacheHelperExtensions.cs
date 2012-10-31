using System;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Umbraco.Core;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;

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
		/// <remarks>
		/// This would be better left internal, however
		/// </remarks>
		public sealed class CacheHelperApplicationEventListener : IApplicationEventHandler
		{
			public void OnApplicationInitialized(UmbracoApplication httpApplication, ApplicationContext applicationContext)
			{
				if (ApplicationContext.Current != null)
				{
					//bind to events to clear the cache, after publish, after media save and after member save

					Document.AfterPublish
						+= (sender, args) =>
						   ApplicationContext.Current.ApplicationCache.ClearPartialViewCache();

					global::umbraco.cms.businesslogic.media.Media.AfterSave
						+= (sender, args) =>
						   ApplicationContext.Current.ApplicationCache.ClearPartialViewCache();

					global::umbraco.cms.businesslogic.member.Member.AfterSave
						+= (sender, args) =>
						   ApplicationContext.Current.ApplicationCache.ClearPartialViewCache();
				}
			}

			public void OnApplicationStarting(UmbracoApplication httpApplication, ApplicationContext applicationContext)
			{
			}

			public void OnApplicationStarted(UmbracoApplication httpApplication, ApplicationContext applicationContext)
			{
			}
		}

		public const string PartialViewCacheKey = "Umbraco.Web.PartialViewCacheKey";

		/// <summary>
		/// Outputs and caches a partial view in MVC
		/// </summary>
		/// <param name="cacheHelper"></param>
		/// <param name="htmlHelper"></param>
		/// <param name="partialViewName"></param>
		/// <param name="model"></param>
		/// <param name="cacheMilliseconds"></param>
		/// <param name="cacheKey">used to cache the partial view, this key could change if it is cached by page or by member</param>
		/// <param name="viewData"></param>
		/// <returns></returns>
		public static IHtmlString CachedPartialView(
			this CacheHelper cacheHelper,
			HtmlHelper htmlHelper,
			string partialViewName,
			object model,
			int cacheMilliseconds,
			string cacheKey,
			ViewDataDictionary viewData = null)
		{
			return cacheHelper.GetCacheItem(
				PartialViewCacheKey + cacheKey,
				CacheItemPriority.NotRemovable, //not removable, the same as macros (apparently issue #27610)
				null,
				new TimeSpan(0, 0, 0, 0, cacheMilliseconds),
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