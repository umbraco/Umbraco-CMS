using System;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Infrastructure;
using Newtonsoft.Json;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Security;
using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Security.Identity
{
    /// <summary>
    /// Used to allow normal Umbraco back office authentication to work
    /// </summary>
    public class UmbracoBackOfficeAuthenticationHandler : AuthenticationHandler<UmbracoBackOfficeCookieAuthenticationOptions>
    {
        private readonly ILogger _logger;
        private bool _shouldRenew;
        private DateTimeOffset _renewIssuedUtc;
        private DateTimeOffset _renewExpiresUtc;

        public UmbracoBackOfficeAuthenticationHandler(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Checks if we should authentication the request (i.e. is back office) and if so gets the forms auth ticket in the request
        /// and returns an AuthenticationTicket based on that.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// It's worth noting that the UmbracoModule still executes and performs the authentication, however this also needs to execute
        /// so that it assigns the new Principal object on the OWIN request:
        /// http://brockallen.com/2013/10/27/host-authentication-and-web-api-with-owin-and-active-vs-passive-authentication-middleware/
        /// </remarks>
        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            if (ShouldAuthRequest())
            {
                var ticket = GetAuthTicket(Request);

                if (ticket == null)
                {
                    _logger.Warn<UmbracoBackOfficeAuthenticationHandler>(@"Unprotect ticket failed");
                    return null;
                }

                DateTimeOffset currentUtc = Options.SystemClock.UtcNow;
                DateTimeOffset? issuedUtc = ticket.Properties.IssuedUtc;
                DateTimeOffset? expiresUtc = ticket.Properties.ExpiresUtc;

                if (expiresUtc != null && expiresUtc.Value < currentUtc)
                {
                    return null;
                }

                if (issuedUtc != null && expiresUtc != null && Options.SlidingExpiration)
                {
                    TimeSpan timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                    TimeSpan timeRemaining = expiresUtc.Value.Subtract(currentUtc);

                    if (timeRemaining < timeElapsed)
                    {
                        _shouldRenew = true;
                        _renewIssuedUtc = currentUtc;
                        TimeSpan timeSpan = expiresUtc.Value.Subtract(issuedUtc.Value);
                        _renewExpiresUtc = currentUtc.Add(timeSpan);
                    }
                }

                var context = new CookieValidateIdentityContext(Context, ticket, Options);

                await Options.Provider.ValidateIdentity(context);

                return new AuthenticationTicket(context.Identity, context.Properties);
            }

            return await Task.FromResult<AuthenticationTicket>(null);
        }

        protected override async Task ApplyResponseGrantAsync()
        {
            AuthenticationResponseGrant signin = Helper.LookupSignIn(Options.AuthenticationType);
            bool shouldSignin = signin != null;
            AuthenticationResponseRevoke signout = Helper.LookupSignOut(Options.AuthenticationType, Options.AuthenticationMode);
            bool shouldSignout = signout != null;

            if (shouldSignin || shouldSignout || _shouldRenew)
            {
                var cookieOptions = new CookieOptions
                {
                    Domain = Options.CookieDomain,
                    HttpOnly = Options.CookieHttpOnly,
                    Path = Options.CookiePath ?? "/",
                };
                if (Options.CookieSecure == CookieSecureOption.SameAsRequest)
                {
                    cookieOptions.Secure = Request.IsSecure;
                }
                else
                {
                    cookieOptions.Secure = Options.CookieSecure == CookieSecureOption.Always;
                }

                if (shouldSignin)
                {
                    var context = new CookieResponseSignInContext(
                        Context,
                        Options,
                        Options.AuthenticationType,
                        signin.Identity,
                        signin.Properties);

                    DateTimeOffset issuedUtc = Options.SystemClock.UtcNow;
                    DateTimeOffset expiresUtc = issuedUtc.Add(Options.ExpireTimeSpan);

                    context.Properties.IssuedUtc = issuedUtc;
                    context.Properties.ExpiresUtc = expiresUtc;

                    Options.Provider.ResponseSignIn(context);

                    if (context.Properties.IsPersistent)
                    {
                        cookieOptions.Expires = expiresUtc.ToUniversalTime().DateTime;
                    }

                    var model = new AuthenticationTicket(context.Identity, context.Properties);
                    string cookieValue = Options.TicketDataFormat.Protect(model);

                    Response.Cookies.Append(
                        Options.CookieName,
                        cookieValue,
                        cookieOptions);
                }
                else if (shouldSignout)
                {
                    Response.Cookies.Delete(
                        Options.CookieName,
                        cookieOptions);
                }
                else if (_shouldRenew)
                {
                    AuthenticationTicket model = await AuthenticateAsync();

                    model.Properties.IssuedUtc = _renewIssuedUtc;
                    model.Properties.ExpiresUtc = _renewExpiresUtc;

                    string cookieValue = Options.TicketDataFormat.Protect(model);

                    if (model.Properties.IsPersistent)
                    {
                        cookieOptions.Expires = _renewExpiresUtc.ToUniversalTime().DateTime;
                    }

                    Response.Cookies.Append(
                        Options.CookieName,
                        cookieValue,
                        cookieOptions);
                }

                //Response.Headers.Set(
                //    HeaderNameCacheControl,
                //    HeaderValueNoCache);

                //Response.Headers.Set(
                //    HeaderNamePragma,
                //    HeaderValueNoCache);

                //Response.Headers.Set(
                //    HeaderNameExpires,
                //    HeaderValueMinusOne);

                bool shouldLoginRedirect = shouldSignin && Options.LoginPath.HasValue && Request.Path == Options.LoginPath;
                bool shouldLogoutRedirect = shouldSignout && Options.LogoutPath.HasValue && Request.Path == Options.LogoutPath;

                if ((shouldLoginRedirect || shouldLogoutRedirect) && Response.StatusCode == 200)
                {
                    IReadableStringCollection query = Request.Query;
                    string redirectUri = query.Get(Options.ReturnUrlParameter);
                    if (!string.IsNullOrWhiteSpace(redirectUri)
                        //&& IsHostRelative(redirectUri)
                        )
                    {
                        var redirectContext = new CookieApplyRedirectContext(Context, Options, redirectUri);
                        Options.Provider.ApplyRedirect(redirectContext);
                    }
                }
            }
        }

        private bool ShouldAuthRequest()
        {
            var httpContext = Context.HttpContextFromOwinContext();

            // do not process if client-side request
            if (httpContext.Request.Url.IsClientSideRequest())
                return false;

            return UmbracoModule.ShouldAuthenticateRequest(httpContext.Request, Request.Uri);
        }

        /// <summary>
        /// Returns the current FormsAuth ticket in the request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private AuthenticationTicket GetAuthTicket(IOwinRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");

            var formsCookie = request.Cookies[Options.CookieName];
            if (string.IsNullOrWhiteSpace(formsCookie))
            {
                return null;
            }
            //get the ticket
            try
            {
                return Options.TicketDataFormat.Unprotect(formsCookie);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}