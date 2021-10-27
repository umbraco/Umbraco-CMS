using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Security;

namespace Umbraco.Web.Security
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
        private readonly IGlobalSettings _globalSettings;
        private readonly ISecuritySection _security;
        private readonly ILogger _logger;

        public GetUserSecondsMiddleWare(
            OwinMiddleware next,
            UmbracoBackOfficeCookieAuthOptions authOptions,
            IGlobalSettings globalSettings,
            ISecuritySection security,
            ILogger logger)
            : base(next)
        {
            _authOptions = authOptions ?? throw new ArgumentNullException(nameof(authOptions));
            _globalSettings = globalSettings;
            _security = security;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task Invoke(IOwinContext context)
        {
            var request = context.Request;
            var response = context.Response;

            if (request.Uri.Scheme.InvariantStartsWith("http")
                && request.Uri.AbsolutePath.InvariantEquals(
                    $"{_globalSettings.Path}/backoffice/UmbracoApi/Authentication/GetRemainingTimeoutSeconds"))
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
                        response.Headers.Add("Cache-Control", new[] { "no-store", "must-revalidate", "no-cache", "max-age=0" });
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
                                    // TODO: This would probably be simpler just to do: context.OwinContext.Authentication.SignIn(context.Properties, identity);
                                    // this will invoke the default Cookie middleware to basically perform this logic for us.

                                    ticket.Properties.IssuedUtc = currentUtc;
                                    var timeSpan = expiresUtc.Value.Subtract(issuedUtc.Value);
                                    ticket.Properties.ExpiresUtc = currentUtc.Add(timeSpan);

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

                            //We also need to re-validate the user's session if we are relying on this ping to keep their session alive
                            await SessionIdValidator.ValidateSessionAsync(TimeSpan.FromMinutes(1), context, _authOptions.CookieManager, _authOptions.SystemClock, issuedUtc, ticket.Identity, _globalSettings);
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

                // HACK: we need to suppress the stupid forms authentication module but we can only do that by using non owin stuff
                if (HttpContext.Current != null && HttpContext.Current.Response != null)
                {
                    HttpContext.Current.Response.SuppressFormsAuthenticationRedirect = true;
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
