using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;

namespace Umbraco.Web.BackOffice.Security
{

    /// <summary>
    /// Antiforgery implementation for the Umbraco back office
    /// </summary>
    /// <remarks>
    /// This is a wrapper around the global/default <see cref="IAntiforgery"/> .net service. Because this service is a single/global
    /// object and all of it is internal we don't have the flexibility to create our own segregated service so we have to work around
    /// that limitation by wrapping the default and doing a few tricks to have this segregated for the Back office only.
    /// </remarks>
    public class BackOfficeAntiforgery : IBackOfficeAntiforgery
    {
        private readonly IAntiforgery _defaultAntiforgery;
        private readonly IOptions<AntiforgeryOptions> _antiforgeryOptions;

        public BackOfficeAntiforgery(
            IAntiforgery defaultAntiforgery,
            IOptions<AntiforgeryOptions> antiforgeryOptions)
        {
            _defaultAntiforgery = defaultAntiforgery;
            _antiforgeryOptions = antiforgeryOptions;
        }

        public async Task<bool> ValidateTokensAsync(HttpContext httpContext, string cookieToken, string headerToken)
        {
            // We need to do some tricks here, save the initial cookie vals, then reset later
            var originalCookies = httpContext.Request.Cookies;
            var originalCookiesHeader = httpContext.Request.Headers[HeaderNames.Cookie];
            var originalHeader = httpContext.Request.Headers[_antiforgeryOptions.Value.HeaderName];
            //var originalForm = httpContext.Request.Form;
            try
            {
                // this is how you write to the request cookies, it's the only way
                var cookieHeaderVals = CookieHeaderValue.ParseList(originalCookiesHeader);
                cookieHeaderVals.Add(new CookieHeaderValue(_antiforgeryOptions.Value.Cookie.Name, cookieToken));
                httpContext.Request.Headers[HeaderNames.Cookie] = cookieHeaderVals.Select(c => c.ToString()).ToArray();

                // change the header/form val to ours
                //var newForm = httpContext.Request.Form.ToDictionary(x => x.Key, x => x.Value);
                //newForm.Add(_antiforgeryOptions.Value.FormFieldName, headerToken);
                //httpContext.Request.Form = new FormCollection(newForm);
                httpContext.Request.Headers[_antiforgeryOptions.Value.HeaderName] = headerToken;

                return await _defaultAntiforgery.IsRequestValidAsync(httpContext);
            }
            finally
            {
                // reset                
                var cookieHeaderVals = CookieHeaderValue.ParseList(originalCookiesHeader);
                httpContext.Request.Headers[HeaderNames.Cookie] = cookieHeaderVals.Select(c => c.ToString()).ToArray();

                if (originalHeader.Count > 0)
                    httpContext.Request.Headers[_antiforgeryOptions.Value.HeaderName] = originalHeader;

                //httpContext.Request.Form = originalForm;
            }
        }

        /// <summary>
        /// Validates the headers/cookies passed in for the request
        /// </summary>
        /// <param name="requestHeaders"></param>
        /// <param name="failedReason"></param>
        /// <returns></returns>
        public async Task<Attempt<string>> ValidateHeadersAsync(HttpContext httpContext)
        {
            httpContext.Request.Cookies.TryGetValue(Constants.Web.CsrfValidationCookieName, out var cookieToken);

            return await ValidateHeadersAsync(
                httpContext,
                cookieToken == null ? null : cookieToken);
        }

        private async Task<Attempt<string>> ValidateHeadersAsync(
            HttpContext httpContext,
            string cookieToken)
        {
            var requestHeaders = httpContext.Request.Headers;
            if (!requestHeaders.TryGetValue(Constants.Web.AngularHeadername, out var headerVals))
            {
                return Attempt.Fail("Missing token");
            }

            var headerToken = headerVals.FirstOrDefault();

            // both header and cookie must be there
            if (cookieToken == null || headerToken == null)
            {
                return Attempt.Fail("Missing token null");
            }

            if (await ValidateTokensAsync(httpContext, cookieToken, headerToken) == false)
            {
                return Attempt.Fail("Invalid token");
            }

            return Attempt<string>.Succeed();
        }

        public void GetTokens(HttpContext httpContext, out string cookieToken, out string headerToken)
        {
            var set = _defaultAntiforgery.GetTokens(httpContext);

            cookieToken = set.RequestToken;
            headerToken = set.RequestToken;
        }


    }
}
