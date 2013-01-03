using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core;

namespace Umbraco.Web
{
	/// <summary>
	/// Extension methods for the HttpRequest and HttpRequestBase objects
	/// </summary>
	public static class HttpRequestExtensions
	{
		/// <summary>
		/// Safely get a request item as string, if the item does not exist, an empty string is returned.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string GetItemAsString(this HttpRequest request, string key)
		{
			return new HttpRequestWrapper(request).GetItemAsString(key);
		}

		/// <summary>
		/// Safely get a request item as string, if the item does not exist, an empty string is returned.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string GetItemAsString(this HttpRequestBase request, string key)
		{
			var val = HttpContext.Current.Request[key];
			return !val.IsNullOrWhiteSpace() ? val : string.Empty;
		}

	}
}
