using System;
using Umbraco.Core;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Infrastructure;

namespace Umbraco.Web.Security.Identity
{
    /// <summary>
    /// If a flag is set on the context to force renew the ticket, this will do it
    /// </summary>
    internal class ForceRenewalCookieAuthenticationHandler : AuthenticationHandler<CookieAuthenticationOptions>
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public ForceRenewalCookieAuthenticationHandler(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// This handler doesn't actually do any auth so we return null;
        /// </summary>
        /// <returns></returns>
        protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            return Task.FromResult((AuthenticationTicket)null);
        }

        /// <summary>
        /// Gets the ticket from the request
        /// </summary>
        /// <returns></returns>
        private AuthenticationTicket GetTicket()
        {
            var cookie = Options.CookieManager.GetRequestCookie(Context, Options.CookieName);
            if (string.IsNullOrWhiteSpace(cookie))
            {
                return null;
            }
            var ticket = Options.TicketDataFormat.Unprotect(cookie);
            if (ticket == null)
            {
                return null;
            }
            return ticket;
        }

        /// <summary>
        /// This will check if the token exists in the request to force renewal
        /// </summary>
        /// <returns></returns>
        protected override Task ApplyResponseGrantAsync()
        {
            if (_umbracoContextAccessor.Value == null || Context.Request.Uri.IsClientSideRequest())
            {
                return Task.FromResult(0);
            }

            //Now we need to check if we should force renew this based on a flag in the context and whether this is a request that is not normally renewed by OWIN...
            // which means that it is not a normal URL that is authenticated.

            var normalAuthUrl = ((BackOfficeCookieManager) Options.CookieManager)
                .ShouldAuthenticateRequest(Context, _umbracoContextAccessor.Value.OriginalRequestUrl,
                    //Pass in false, we want to know if this is a normal auth'd page
                    checkForceAuthTokens: false);
            //This is auth'd normally, so OWIN will naturally take care of the cookie renewal
            if (normalAuthUrl) return Task.FromResult(0);

            var httpCtx = Context.TryGetHttpContext();
            //check for the special flag in either the owin or http context
            var shouldRenew = Context.Get<bool?>("umbraco-force-auth") != null || (httpCtx.Success && httpCtx.Result.Items["umbraco-force-auth"] != null);
            
            if (shouldRenew)
            {
                var signin = Helper.LookupSignIn(Options.AuthenticationType);
                var shouldSignin = signin != null;
                var signout = Helper.LookupSignOut(Options.AuthenticationType, Options.AuthenticationMode);
                var shouldSignout = signout != null;

                //we don't care about the sign in/sign out scenario, only renewal
                if (shouldSignin == false && shouldSignout == false)
                {
                    //get the ticket
                    var ticket = GetTicket();
                    if (ticket != null)
                    {
                        var currentUtc = Options.SystemClock.UtcNow;
                        var issuedUtc = ticket.Properties.IssuedUtc;
                        var expiresUtc = ticket.Properties.ExpiresUtc;

                        if (expiresUtc.HasValue && issuedUtc.HasValue)
                        {
                            var timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                            var timeRemaining = expiresUtc.Value.Subtract(currentUtc);

                            //if it's time to renew, then do it
                            if (timeRemaining < timeElapsed)
                            {
                                //renew the date/times
                                ticket.Properties.IssuedUtc = currentUtc;
                                var timeSpan = expiresUtc.Value.Subtract(issuedUtc.Value);
                                ticket.Properties.ExpiresUtc = currentUtc.Add(timeSpan);

                                //now save back all the required cookie details
                                var cookieValue = Options.TicketDataFormat.Protect(ticket);

                                var cookieOptions = ((UmbracoBackOfficeCookieAuthOptions)Options).CreateRequestCookieOptions(Context, ticket);                                

                                Options.CookieManager.AppendResponseCookie(
                                    Context,
                                    Options.CookieName,
                                    cookieValue,
                                    cookieOptions);
                            }                            

                            
                        }
                    }
                }
            }

            return Task.FromResult(0);
        }
    }
}