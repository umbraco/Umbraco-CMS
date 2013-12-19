using System;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Umbraco.Core;
using umbraco.cms.businesslogic.web;

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
