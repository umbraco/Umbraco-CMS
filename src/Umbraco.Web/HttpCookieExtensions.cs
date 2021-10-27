using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Microsoft.Owin;
using Newtonsoft.Json;
using Umbraco.Core;

namespace Umbraco.Web
{
    /// <summary>
    /// Extension methods used to check/set cookie values
    /// </summary>
    /// <remarks>
    /// This should 100% supersede the StateManager.Cookies
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

                var cookieCollection = cookiesHeaderValue.Split(Constants.CharArrays.Semicolon, StringSplitOptions.RemoveEmptyEntries);
                foreach (var cookieNameValue in cookieCollection)
                {
                    var parts = cookieNameValue.Split(Constants.CharArrays.EqualsChar, StringSplitOptions.RemoveEmptyEntries);
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
            var cookie = http.Response.Cookies[cookieName];
            if (cookie != null)
            {
                //this will expire immediately and be removed from the browser
                cookie.Expires = DateTime.Now.AddYears(-1);
            }
            else
            {
                //ensure there's def an expired cookie
                http.Response.Cookies.Add(new HttpCookie(cookieName) { Expires = DateTime.Now.AddYears(-1) });
            }
        }

        /// <summary>
        /// Removes the cookie from the request and the response if it exists
        /// </summary>
        /// <param name="http"></param>
        /// <param name="cookieName"></param>
        public static void ExpireCookie(this HttpContext http, string cookieName)
        {
            new HttpContextWrapper(http).ExpireCookie(cookieName);
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

        public static string GetPreviewCookieValue(this HttpRequestBase request)
        {
            return request.GetCookieValue(Constants.Web.PreviewCookieName);
        }

        public static string GetPreviewCookieValue(this HttpRequest request)
        {
            return new HttpRequestWrapper(request).GetPreviewCookieValue();
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
        public static bool HasPreviewCookie(this HttpRequest request)
        {
            return new HttpRequestWrapper(request).HasPreviewCookie();
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
        /// Returns the cookie's string value
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        public static string GetCookieValue(this HttpRequestBase request, string cookieName)
        {
            var cookie = request.Cookies.Get(cookieName);
            if (cookie != null)
            {
                if (cookie.Value.IsNullOrWhiteSpace() == false)
                {
                    return cookie.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the cookie's string value
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        public static string GetCookieValue(this HttpRequest request, string cookieName)
        {
            return new HttpRequestWrapper(request).GetCookieValue(cookieName);
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
        /// Does a cookie exist with the specified key ?
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasCookie(this HttpRequest request, string key)
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

        /// <summary>
        /// Is there a cookie with the key supplied and does it have a value that is not empty
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasCookieValue(this HttpRequest request, string key)
        {
            return new HttpRequestWrapper(request).HasCookieValue(key);
        }
    }
}
