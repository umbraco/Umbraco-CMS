using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Security;

namespace Umbraco.Web
{
    public static class ViewDataExtensions
    {
        public const string TokenUmbracoPath = "UmbracoPath";
        public const string TokenInstallApiBaseUrl = "InstallApiBaseUrl";
        public const string TokenUmbracoBaseFolder = "UmbracoBaseFolder";
        public const string TokenExternalSignInError = "ExternalSignInError";
        public const string TokenPasswordResetCode = "PasswordResetCode";

        public static bool FromTempData(this ViewDataDictionary viewData, TempDataDictionary tempData, string token)
        {
            if (tempData[token] == null) return false;
            viewData[token] = tempData[token];
            return true;
        }

        /// <summary>
        /// Copies data from a request cookie to view data and then clears the cookie in the response
        /// </summary>
        /// <param name="viewData"></param>
        /// <param name="httpContext"></param>
        /// <param name="cookieName"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This is similar to TempData but in some cases we cannot use TempData which relies on the temp data provider and session.
        /// The cookie value can either be a simple string value
        /// </para>
        /// </remarks>
        internal static bool FromBase64CookieData<T>(this ViewDataDictionary viewData, HttpContextBase httpContext, string cookieName)
        {
            var hasCookie = httpContext.Request.HasCookie(cookieName);
            if (!hasCookie) return false;

            // get the cookie value
            var cookieVal = httpContext.Request.GetCookieValue(cookieName);

            if (cookieVal == null)
                return false;

            // ensure the cookie is expired (must be done after reading the value)
            httpContext.ExpireCookie(cookieName);

            if (cookieVal.IsNullOrWhiteSpace())
                return false;

            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(System.Net.WebUtility.UrlDecode(cookieVal)));
                // deserialize to T and store in viewdata
                viewData[cookieName] = JsonConvert.DeserializeObject<T>(decoded);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetUmbracoPath(this ViewDataDictionary viewData)
        {
            return (string)viewData[TokenUmbracoPath];
        }

        public static void SetUmbracoPath(this ViewDataDictionary viewData, string value)
        {
            viewData[TokenUmbracoPath] = value;
        }

        public static string GetInstallApiBaseUrl(this ViewDataDictionary viewData)
        {
            return (string)viewData[TokenInstallApiBaseUrl];
        }

        public static void SetInstallApiBaseUrl(this ViewDataDictionary viewData, string value)
        {
            viewData[TokenInstallApiBaseUrl] = value;
        }

        public static string GetUmbracoBaseFolder(this ViewDataDictionary viewData)
        {
            return (string)viewData[TokenUmbracoBaseFolder];
        }

        public static void SetUmbracoBaseFolder(this ViewDataDictionary viewData, string value)
        {
            viewData[TokenUmbracoBaseFolder] = value;
        }

        /// <summary>
        /// Used by the back office login screen to get any registered external login provider errors
        /// </summary>
        /// <param name="viewData"></param>
        /// <returns></returns>
        public static BackOfficeExternalLoginProviderErrors GetExternalSignInProviderErrors(this ViewDataDictionary viewData)
        {
            return (BackOfficeExternalLoginProviderErrors)viewData[TokenExternalSignInError];
        }

        [Obsolete("Use GetExternalSignInProviderErrors instead")]
        public static IEnumerable<string> GetExternalSignInError(this ViewDataDictionary viewData)
        {
            var errs = viewData.GetExternalSignInProviderErrors();
            return errs?.Errors ?? Enumerable.Empty<string>();
        }

        /// <summary>
        /// Used by the back office controller to register any external login provider errors
        /// </summary>
        /// <param name="viewData"></param>
        /// <param name="errors"></param>
        public static void SetExternalSignInProviderErrors(this ViewDataDictionary viewData, BackOfficeExternalLoginProviderErrors errors)
        {
            viewData[TokenExternalSignInError] = errors;
        }

        [Obsolete("Use SetExternalSignInProviderErrors instead")]
        public static void SetExternalSignInError(this ViewDataDictionary viewData, IEnumerable<string> value)
        {
            viewData[TokenExternalSignInError] = new BackOfficeExternalLoginProviderErrors(string.Empty, value);
        }

        public static string GetPasswordResetCode(this ViewDataDictionary viewData)
        {
            return (string)viewData[TokenPasswordResetCode];
        }

        public static void SetPasswordResetCode(this ViewDataDictionary viewData, string value)
        {
            viewData[TokenPasswordResetCode] = value;
        }
    }
}
