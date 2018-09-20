﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Helpers;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Composing;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// A helper class to deal with csrf prevention with angularjs and webapi
    /// </summary>
    public static class AngularAntiForgeryHelper
    {
        /// <summary>
        /// The cookie name that is used to store the validation value
        /// </summary>
        public const string CsrfValidationCookieName = "UMB-XSRF-V";

        /// <summary>
        /// The cookie name that is set for angular to use to pass in to the header value for "X-UMB-XSRF-TOKEN"
        /// </summary>
        public const string AngularCookieName = "UMB-XSRF-TOKEN";

        /// <summary>
        /// The header name that angular uses to pass in the token to validate the cookie
        /// </summary>
        public const string AngularHeadername = "X-UMB-XSRF-TOKEN";

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
        public static void GetTokens(out string cookieToken, out string headerToken)
        {
            AntiForgery.GetTokens(null, out cookieToken, out headerToken);
        }

        /// <summary>
        /// Validates the header token against the validation cookie value
        /// </summary>
        /// <param name="cookieToken"></param>
        /// <param name="headerToken"></param>
        /// <returns></returns>
        public static bool ValidateTokens(string cookieToken, string headerToken)
        {
            // ensure that the cookie matches the header and then ensure it matches the correct value!
            try
            {
                AntiForgery.Validate(cookieToken, headerToken);
            }
            catch (Exception ex)
            {
                Current.Logger.Error(typeof(AngularAntiForgeryHelper), ex, "Could not validate XSRF token");
                return false;
            }
            return true;
        }

        internal static bool ValidateHeaders(
            KeyValuePair<string, IEnumerable<string>>[] requestHeaders,
            string cookieToken,
            out string failedReason)
        {
            failedReason = "";

            if (requestHeaders.Any(z => z.Key.InvariantEquals(AngularHeadername)) == false)
            {
                failedReason = "Missing token";
                return false;
            }

            var headerToken = requestHeaders
                .Where(z => z.Key.InvariantEquals(AngularHeadername))
                .Select(z => z.Value)
                .SelectMany(z => z)
                .FirstOrDefault();

            // both header and cookie must be there
            if (cookieToken == null || headerToken == null)
            {
                failedReason = "Missing token null";
                return false;
            }

            if (ValidateTokens(cookieToken, headerToken) == false)
            {
                failedReason = "Invalid token";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the headers/cookies passed in for the request
        /// </summary>
        /// <param name="requestHeaders"></param>
        /// <param name="failedReason"></param>
        /// <returns></returns>
        public static bool ValidateHeaders(HttpRequestHeaders requestHeaders, out string failedReason)
        {
            var cookieToken = requestHeaders.GetCookieValue(CsrfValidationCookieName);

            return ValidateHeaders(
                requestHeaders.ToDictionary(x => x.Key, x => x.Value).ToArray(),
                cookieToken == null ? null : cookieToken,
                out failedReason);
        }

    }
}
