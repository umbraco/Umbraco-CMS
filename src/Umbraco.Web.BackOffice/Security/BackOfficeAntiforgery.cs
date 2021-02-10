using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Umbraco.Cms.Core;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Security
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

        /// <inheritdoc />
        public async Task<Attempt<string>> ValidateRequestAsync(HttpContext httpContext)
        {
            if (!httpContext.Request.Headers.TryGetValue(Constants.Web.AngularHeadername, out var headerVals))
            {
                return Attempt.Fail("Missing header");
            }
            if (!httpContext.Request.Cookies.TryGetValue(Constants.Web.CsrfValidationCookieName, out var cookieToken))
            {
                return Attempt.Fail("Missing cookie");
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

        /// <inheritdoc />
        public void GetTokens(HttpContext httpContext, out string cookieToken, out string headerToken)
        {
            var set = _defaultAntiforgery.GetTokens(httpContext);

            cookieToken = set.CookieToken;
            headerToken = set.RequestToken;
        }

        /// <summary>
        /// Validates the cookie and header tokens
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="cookieToken"></param>
        /// <param name="headerToken"></param>
        /// <returns></returns>
        private async Task<bool> ValidateTokensAsync(HttpContext httpContext, string cookieToken, string headerToken)
        {
            // TODO: see https://github.com/dotnet/aspnetcore/issues/22217
            // An alternative way to doing this would be to create a separate container specifically for antiforgery and add custom options to
            // it and resolve services directly from there. Could be worth a shot and could actually be a way for us to deal with these global
            // things later on if this problem arises again.

            // We need to do some tricks here, save the initial cookie/header vals, then reset later
            var originalCookies = httpContext.Request.Cookies;
            var originalCookiesHeader = httpContext.Request.Headers[HeaderNames.Cookie];
            var originalHeader = httpContext.Request.Headers[_antiforgeryOptions.Value.HeaderName];

            try
            {
                // swap the cookie anti-forgery cookie value for the one we want to validate
                // (this is how you modify request cookies, it's the only way)
                var cookieHeaderVals = CookieHeaderValue.ParseList(originalCookiesHeader);
                cookieHeaderVals.Add(new CookieHeaderValue(_antiforgeryOptions.Value.Cookie.Name, cookieToken));
                httpContext.Request.Headers[HeaderNames.Cookie] = cookieHeaderVals.Select(c => c.ToString()).ToArray();

                // swap the anti-forgery header value for the one we want to validate
                httpContext.Request.Headers[_antiforgeryOptions.Value.HeaderName] = headerToken;

                // now validate
                return await _defaultAntiforgery.IsRequestValidAsync(httpContext);
            }
            finally
            {
                // reset

                // change request cookies back to original
                var cookieHeaderVals = CookieHeaderValue.ParseList(originalCookiesHeader);
                httpContext.Request.Headers[HeaderNames.Cookie] = cookieHeaderVals.Select(c => c.ToString()).ToArray();

                // change the header back to normal
                httpContext.Request.Headers[_antiforgeryOptions.Value.HeaderName] = originalHeader;
            }
        }


    }
}
