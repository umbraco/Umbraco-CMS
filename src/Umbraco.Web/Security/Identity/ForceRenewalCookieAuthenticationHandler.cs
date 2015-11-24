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
            //Now we need to check if we should force renew this based on a flag in the context

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
                    var model = GetTicket();
                    if (model != null)
                    {
                        var currentUtc = Options.SystemClock.UtcNow;
                        var issuedUtc = model.Properties.IssuedUtc;
                        var expiresUtc = model.Properties.ExpiresUtc;

                        if (expiresUtc.HasValue && issuedUtc.HasValue)
                        {
                            //renew the date/times
                            model.Properties.IssuedUtc = currentUtc;
                            var timeSpan = expiresUtc.Value.Subtract(issuedUtc.Value);
                            model.Properties.ExpiresUtc = currentUtc.Add(timeSpan);

                            //now save back all the required cookie details
                            var cookieValue = Options.TicketDataFormat.Protect(model);
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
                            if (model.Properties.IsPersistent)
                            {
                                cookieOptions.Expires = model.Properties.ExpiresUtc.Value.ToUniversalTime().DateTime;
                            }
                            Options.CookieManager.AppendResponseCookie(
                                Context,
                                Options.CookieName,
                                cookieValue,
                                cookieOptions);
                        }
                    }
                }
            }

            return Task.FromResult(0);
        }
    }
}