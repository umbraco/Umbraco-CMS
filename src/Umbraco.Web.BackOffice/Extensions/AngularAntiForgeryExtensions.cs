using System;
using System.Collections.Generic;
using Umbraco.Core;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace Umbraco.Extensions
{
    /// <summary>
    /// A helper class to deal with csrf prevention with angularjs and webapi
    /// </summary>
    public static class AngularAntiForgeryExtensions
    {
        /// <summary>
        /// Returns 2 tokens - one for the cookie value and one that angular should set as the header value
        /// </summary>
        /// <param name="cookieToken"></param>
        /// <param name="headerToken"></param>
        /// <remarks>
        /// .Net provides us a way to validate one token with another for added security. With the way angular works, this
        /// means that we need to set 2 cookies since angular uses one cookie value to create the header value, then we want to validate
        /// this header value against our original cookie value.
        /// </remarks>
        public static void GetTokens(this IAntiforgery antiforgery, HttpContext httpContext, out string cookieToken, out string headerToken)
        {
            var result = antiforgery.GetTokens(httpContext);

            cookieToken = result.CookieToken;
            headerToken = result.RequestToken;
        }

        ///// <summary>
        ///// Validates the header token against the validation cookie value
        ///// </summary>
        ///// <param name="cookieToken"></param>
        ///// <param name="headerToken"></param>
        ///// <returns></returns>
        //public static bool ValidateTokens(this IAntiforgery antiforgery, HttpContext httpContext, string cookieToken, string headerToken)
        //{
        //    // ensure that the cookie matches the header and then ensure it matches the correct value!
        //    try
        //    {
        //        antiforgery.Va .Validate(cookieToken, headerToken);
        //    }
        //    catch (Exception ex)
        //    {
        //        Current.Logger.Error(typeof(AngularAntiForgeryHelper), ex, "Could not validate XSRF token");
        //        return false;
        //    }
        //    return true;
        //}

        //internal static bool ValidateHeaders(
        //    KeyValuePair<string, IEnumerable<string>>[] requestHeaders,
        //    string cookieToken,
        //    out string failedReason)
        //{
        //    failedReason = "";

        //    if (requestHeaders.Any(z => z.Key.InvariantEquals(Constants.Web.AngularHeadername)) == false)
        //    {
        //        failedReason = "Missing token";
        //        return false;
        //    }

        //    var headerToken = requestHeaders
        //        .Where(z => z.Key.InvariantEquals(Constants.Web.AngularHeadername))
        //        .Select(z => z.Value)
        //        .SelectMany(z => z)
        //        .FirstOrDefault();

        //    // both header and cookie must be there
        //    if (cookieToken == null || headerToken == null)
        //    {
        //        failedReason = "Missing token null";
        //        return false;
        //    }

        //    if (ValidateTokens(cookieToken, headerToken) == false)
        //    {
        //        failedReason = "Invalid token";
        //        return false;
        //    }

        //    return true;
        //}

        ///// <summary>
        ///// Validates the headers/cookies passed in for the request
        ///// </summary>
        ///// <param name="requestHeaders"></param>
        ///// <param name="failedReason"></param>
        ///// <returns></returns>
        //public static bool ValidateHeaders(HttpRequestHeaders requestHeaders, out string failedReason)
        //{
        //    var cookieToken = requestHeaders.GetCookieValue(Constants.Web.CsrfValidationCookieName);

        //    return ValidateHeaders(
        //        requestHeaders.ToDictionary(x => x.Key, x => x.Value).ToArray(),
        //        cookieToken == null ? null : cookieToken,
        //        out failedReason);
        //}

    }
}
