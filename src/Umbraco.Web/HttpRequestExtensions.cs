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
        /// Extracts the value from the query string and cleans it to prevent xss attacks.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>        
        public static string CleanForXss(this HttpRequest request, string key)
        {
            var item = request.GetItemAsString(key);
            return item.CleanForXss();
        }

		/// <summary>
		/// Safely get a request item as string, if the item does not exist, an empty string is returned.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="key"></param>
		/// <param name="valueIfNotFound">The value to return if the key is not found in the collection</param>
		/// <returns></returns>
		public static string GetItemAsString(this HttpRequest request, string key, string valueIfNotFound = "")
		{
			return new HttpRequestWrapper(request).GetItemAsString(key, valueIfNotFound);
		}

		/// <summary>
		/// Safely get a request item as string, if the item does not exist, an empty string is returned.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="key"></param>
		/// <param name="valueIfNotFound">The value to return if the key is not found in the collection</param>
		/// <returns></returns>
		public static string GetItemAsString(this HttpRequestBase request, string key, string valueIfNotFound = "")
		{
			var val = request[key];
			return !val.IsNullOrWhiteSpace() ? val : valueIfNotFound;
		}

		/// <summary>
		/// Safely get the item from the query string and convert it to type 'T', otherwise will return default(T).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="request"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static T GetItemAs<T>(this HttpRequestBase request, string key)
		{
			var val = request[key];
			var whitespaceCheck = !val.IsNullOrWhiteSpace() ? val : string.Empty;
			if (whitespaceCheck.IsNullOrWhiteSpace())
				return (T) typeof (T).GetDefaultValue();
			var attempt = val.TryConvertTo<T>();
			if (attempt.Success)
				return attempt.Result;
			return (T)typeof(T).GetDefaultValue();
		}

		/// <summary>
		/// Safely get the item from the query string and convert it to type 'T', otherwise will return default(T).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="request"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static T GetItemAs<T>(this HttpRequest request, string key)
		{
			return new HttpRequestWrapper(request).GetItemAs<T>(key);
		}

	}
}
