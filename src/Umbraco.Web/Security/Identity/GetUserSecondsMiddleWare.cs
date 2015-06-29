using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Web.Security.Identity
{
    /// <summary>
    /// Custom middleware to return the remaining seconds the user has before they are logged out
    /// </summary>
    /// <remarks>
    /// This is quite a custom request because in most situations we just want to return the seconds and don't want
    /// to renew the auth ticket, however if KeepUserLoggedIn is true, then we do want to renew the auth ticket for
    /// this request!
    /// </remarks>
    internal class GetUserSecondsMiddleWare : OwinMiddleware
    {
        private readonly UmbracoBackOfficeCookieAuthOptions _authOptions;
        private readonly ISecuritySection _security;

        public GetUserSecondsMiddleWare(
            OwinMiddleware next,
            UmbracoBackOfficeCookieAuthOptions authOptions,
            ISecuritySection security)
            : base(next)
        {
            _authOptions = authOptions;
            _security = security;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var request = context.Request;
            var response = context.Response;

            var rootPath = context.Request.PathBase.HasValue
                ? context.Request.PathBase.Value.EnsureStartsWith("/").EnsureEndsWith("/")
                : "/";

            if (request.Uri.Scheme.InvariantStartsWith("http")
                && request.Uri.AbsolutePath.InvariantEquals(
                    string.Format("{0}{1}/backoffice/UmbracoApi/Authentication/GetRemainingTimeoutSeconds", rootPath, GlobalSettings.UmbracoMvcArea)))
            {
                var cookie = _authOptions.CookieManager.GetRequestCookie(context, _security.AuthCookieName);
                if (cookie.IsNullOrWhiteSpace() == false)
                {
                    var ticket = _authOptions.TicketDataFormat.Unprotect(cookie);
                    if (ticket != null)
                    {
                        var remainingSeconds = ticket.Properties.ExpiresUtc.HasValue
                            ? (ticket.Properties.ExpiresUtc.Value - DateTime.Now.ToUniversalTime()).TotalSeconds
                            : 0;

                        response.ContentType = "application/json; charset=utf-8";
                        response.StatusCode = 200;
                        response.Headers.Add("Cache-Control", new[] { "no-cache" });
                        response.Headers.Add("Pragma", new[] { "no-cache" });
                        response.Headers.Add("Expires", new[] { "-1" });
                        response.Headers.Add("Date", new[] { DateTime.Now.ToUniversalTime().ToString("R") });

                        //Ok, so here we need to check if we want to process/renew the auth ticket for each 
                        // of these requests. If that is the case, the user will really never be logged out until they
                        // close their browser (there will be edge cases of that, especially when debugging)
                        if (_security.KeepUserLoggedIn)
                        {
                            var utcNow = DateTime.Now.ToUniversalTime();
                            ticket.Properties.IssuedUtc = utcNow;
                            ticket.Properties.ExpiresUtc = utcNow.AddMinutes(_authOptions.LoginTimeoutMinutes);

                            var cookieValue = _authOptions.TicketDataFormat.Protect(ticket);

                            var cookieOptions = new CookieOptions
                            {
                                Path = "/",
                                Domain = _authOptions.CookieDomain,
                                Expires = DateTime.Now.AddMinutes(_authOptions.LoginTimeoutMinutes),
                                HttpOnly = true,
                                Secure = _authOptions.CookieSecure == CookieSecureOption.Always
                                         || (_authOptions.CookieSecure == CookieSecureOption.SameAsRequest && request.Uri.Scheme.InvariantEquals("https")),
                            };

                            _authOptions.CookieManager.AppendResponseCookie(
                                context,
                                _authOptions.CookieName,
                                cookieValue,
                                cookieOptions);
                        }

                        await response.WriteAsync(remainingSeconds.ToString(CultureInfo.InvariantCulture));
                        return;
                    }
                }
                response.StatusCode = 401;
            }
            else if (Next != null)
            {
                await Next.Invoke(context);
            }
        }
    }
}