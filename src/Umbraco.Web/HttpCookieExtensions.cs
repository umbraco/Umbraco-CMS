using System.Web;
using Umbraco.Core;

namespace Umbraco.Web
{
    /// <summary>
    /// Extension methods used to check/set cookie values
    /// </summary>
    /// <remarks>
    /// This should 100% supercede the StateManager.Cookies
    /// </remarks> 
    internal static class HttpCookieExtensions
    {
        internal const string PreviewCookieName = "UMB_PREVIEW";

        /// <summary>
        /// Does a preview cookie exist ?
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool HasPreviewCookie(this HttpRequestBase request)
        {
            return request.Cookies[PreviewCookieName] != null;
        }

        /// <summary>
        /// Does a cookie exist with the specified key ?
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasCookie(this HttpRequestBase request, string key)
        {
            return request.Cookies[key] != null;
        }

        /// <summary>
        /// Is there a cookie with the key supplied and does it have a value that is not empty
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasCookieValue(this HttpRequestBase request, string key)
        {
            return request.Cookies[key] != null
                   && request.Cookies[key].Value != null
                   && request.Cookies[key].Value.IsNullOrWhiteSpace() == false;
        }
    }
}