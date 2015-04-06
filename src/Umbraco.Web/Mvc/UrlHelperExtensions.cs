using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Core.Configuration;

namespace Umbraco.Core
{
	/// <summary>
	/// Extension methods for UrlHelper
	/// </summary>
	public static class UrlHelperExtensions
	{

		/// <summary>
		/// Returns the base path (not including the 'action') of the MVC controller "SaveFileController"
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string GetSaveFileServicePath(this UrlHelper url)
		{
			var result = url.Action("SavePartialView", "SaveFile", new {area = GlobalSettings.UmbracoMvcArea});
            return result.TrimEnd("SavePartialView").EnsureEndsWith('/');
		}

        /// <summary>
        /// Returns the base path (not including the 'action') of the MVC controller "BulkPublishController"
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetBulkPublishServicePath(this UrlHelper url)
        {
            var result = url.Action("PublishDocument", "BulkPublish", new { area = GlobalSettings.UmbracoMvcArea });
            return result.TrimEnd("PublishDocument").EnsureEndsWith('/');
        }

        /// <summary>
        /// Returns the base path (not including the 'action') of the MVC controller "CoreStringsController"
        /// </summary>
        /// <param name="url">The url helper.</param>
        /// <returns>The base path of the controller.</returns>
        public static string GetCoreStringsControllerPath(this UrlHelper url)
        {
            var result = url.Action("ToSafeAlias", "CoreStrings", new { area = GlobalSettings.UmbracoMvcArea });
            return result.TrimEnd("ToSafeAlias");
        }
	}
}
