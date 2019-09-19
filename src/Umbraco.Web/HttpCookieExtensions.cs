using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Microsoft.Owin;
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
        /// <summary>
        /// Retrieves an individual cookie from the cookies collection
        /// </summary>
        /// <param name="requestHeaders"></param>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        /// <remarks>
        /// Adapted from: https://stackoverflow.com/a/29057304/5018 because there's an issue with .NET WebApi cookie parsing logic 
        /// when using requestHeaders.GetCookies() when an invalid cookie name is present.
        /// </remarks>
        public static string GetCookieValue(this HttpRequestHeaders requestHeaders, string cookieName)
        {
            foreach (var header in requestHeaders)
            {
                if (header.Key.Equals("Cookie", StringComparison.InvariantCultureIgnoreCase) == false)
                    continue;
                
                var cookiesHeaderValue = header.Value.FirstOrDefault();
                if (cookiesHeaderValue == null)
                    return null;

                var cookieCollection = cookiesHeaderValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var cookieNameValue in cookieCollection)
                {
                    var parts = cookieNameValue.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length != 2) continue;
                    if (parts[0].Trim().Equals(cookieName, StringComparison.InvariantCultureIgnoreCase))
                        return parts[1].Trim();
                }
            }

            return null;
        }

        /// <summary>
        /// Removes the cookie from the request and the response if it exists
        /// </summary>
        /// <param name="http"></param>
        /// <param name="cookieName"></param>
        public static void ExpireCookie(this HttpContextBase http, string cookieName)
        {
            //remove from the request
            http.Request.Cookies.Remove(cookieName);

            //expire from the response
            var angularCookie = http.Response.Cookies[cookieName];
            if (angularCookie != null)
            {
                //this will expire immediately and be removed from the browser
                angularCookie.Expires = DateTime.Now.AddYears(-1);
            }
            else
            {
                //ensure there's def an expired cookie
                http.Response.Cookies.Add(new HttpCookie(cookieName) { Expires = DateTime.Now.AddYears(-1) });
            }
        }

        public static string GetPreviewCookieValue(this HttpRequestMessage request)
        {
            var cookie = request.Headers.GetCookies(Constants.Web.PreviewCookieName).FirstOrDefault();
            if (cookie != null)
            {
                if (cookie[Constants.Web.PreviewCookieName] != null)
                {
                    return cookie[Constants.Web.PreviewCookieName].Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Does a preview cookie exist ?
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool HasPreviewCookie(this HttpRequestBase request)
        {
            return request.Cookies[Constants.Web.PreviewCookieName] != null;
        }

        /// <summary>
        /// Does a preview cookie exist ?
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool HasPreviewCookie(this IOwinRequest request)
        {
            return request.Cookies[Constants.Web.PreviewCookieName] != null;
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