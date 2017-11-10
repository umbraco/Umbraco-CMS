using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core;
using Umbraco.Web.Routing;

namespace Umbraco.Web
{
    /// <summary>
	/// Extension methods for the HttpRequest and HttpRequestBase objects
	/// </summary>
	public static class HttpRequestExtensions
	{
        /// <summary>
        /// Gets the left part of a URL
        /// </summary>
        /// <param name="request">The request determines both the schema and the URI to resolve the left part for</param>
        /// <param name="leftPart"></param>
        /// <returns></returns>
        /// <remarks>
        /// The scheme is determined by the data found in the request, not just based on the request's URI
        /// </remarks>
        internal static string GetLeftUriPart(this HttpRequestBase request, UriPartial leftPart)
        {
            return request.GetLeftUriPart(request.Url, leftPart);
        }

	    /// <summary>
	    /// Gets the left part of a URL
	    /// </summary>
	    /// <param name="request">The request determines the scheme of the left part</param>
	    /// <param name="uri">An explicit URI to get the left part for</param>
	    /// <param name="leftPart"></param>
	    /// <returns></returns>
	    /// <remarks>
	    /// The scheme is determined by the data found in the request, not just based on the request's URI
	    /// </remarks>
	    internal static string GetLeftUriPart(this HttpRequestBase request, Uri uri, UriPartial leftPart)
	    {
	        if (request == null) throw new ArgumentNullException("request");
	        if (uri == null) throw new ArgumentException("request.uri");

	        var left = uri.GetLeftPart(leftPart);
	        var withoutScheme = left.Substring(uri.Scheme.Length);

	        //TODO: This should call into a service/resolve (i.e. ISecureRequest)
	        var scheme = request.GetScheme();

	        return scheme + withoutScheme;
	    }

        /// <summary>
        /// Returns either https or http depending on the information in the request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetScheme(this HttpRequestBase request)
	    {            
	        return GetScheme(request, DefaultUrlProvider.KnownForwardedHttpsHeaders);
	    }

        /// <summary>
        /// Returns either https or http depending on the information in the request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="knownForwardedHttpsHeaders"></param>
        /// <returns></returns>
        public static string GetScheme(this HttpRequestBase request, string[] knownForwardedHttpsHeaders)
	    {
	        var httpsForwardHeaderExists = false;
	        if (request.Headers != null)
	        {
	            foreach (var header in knownForwardedHttpsHeaders)
	            {
	                httpsForwardHeaderExists = request.Headers.ContainsKey(header);
	                if (httpsForwardHeaderExists) break;
	            }
            }

	        return httpsForwardHeaderExists || request.IsSecureConnection ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
	    }


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
