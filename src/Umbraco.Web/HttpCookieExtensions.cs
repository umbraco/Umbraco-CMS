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
