using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
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
        private readonly ILogger _logger;
        private const int PersistentLoginSlidingMinutes = 30;

        public GetUserSecondsMiddleWare(
            OwinMiddleware next,
            UmbracoBackOfficeCookieAuthOptions authOptions,
            ISecuritySection security,
            ILogger logger)
            : base(next)
        {
            if (authOptions == null) throw new ArgumentNullException("authOptions");
            if (logger == null) throw new ArgumentNullException("logger");
            _authOptions = authOptions;
            _security = security;
            _logger = logger;
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
                            ? (ticket.Properties.ExpiresUtc.Value - _authOptions.SystemClock.UtcNow).TotalSeconds
                            : 0;

                        response.ContentType = "application/json; charset=utf-8";
                        response.StatusCode = 200;
                        response.Headers.Add("Cache-Control", new[] { "no-cache" });
                        response.Headers.Add("Pragma", new[] { "no-cache" });
                        response.Headers.Add("Expires", new[] { "-1" });
                        response.Headers.Add("Date", new[] { _authOptions.SystemClock.UtcNow.ToString("R") });

                        //Ok, so here we need to check if we want to process/renew the auth ticket for each 
                        // of these requests. If that is the case, the user will really never be logged out until they
                        // close their browser (there will be edge cases of that, especially when debugging)
                        if (_security.KeepUserLoggedIn)
                        {
                            var currentUtc = _authOptions.SystemClock.UtcNow;
                            var issuedUtc = ticket.Properties.IssuedUtc;
                            var expiresUtc = ticket.Properties.ExpiresUtc;

                            if (expiresUtc.HasValue && issuedUtc.HasValue)
                            {
                                var timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                                var timeRemaining = expiresUtc.Value.Subtract(currentUtc);

                                //if it's time to renew, then do it
                                if (timeRemaining < timeElapsed)
                                {
                                    ticket.Properties.IssuedUtc = currentUtc;
                                    ticket.Properties.ExpiresUtc = currentUtc.AddMinutes(PersistentLoginSlidingMinutes);

                                    var cookieValue = _authOptions.TicketDataFormat.Protect(ticket);

                                    var cookieOptions = _authOptions.CreateRequestCookieOptions(context, ticket);

                                    _authOptions.CookieManager.AppendResponseCookie(
                                        context,
                                        _authOptions.CookieName,
                                        cookieValue,
                                        cookieOptions);

                                    remainingSeconds = (ticket.Properties.ExpiresUtc.Value - currentUtc).TotalSeconds;
                                }
                            }                            
                        }
                        else if (remainingSeconds <= 30)
                        {
                            //NOTE: We are using 30 seconds because that is what is coded into angular to force logout to give some headway in
                            // the timeout process.

                            _logger.WriteCore(TraceEventType.Information, 0,
                                string.Format("User logged will be logged out due to timeout: {0}, IP Address: {1}", ticket.Identity.Name, request.RemoteIpAddress),
                                null, null);
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