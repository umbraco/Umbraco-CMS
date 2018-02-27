using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security.Cookies;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// Static helper class used to configure a CookieAuthenticationProvider to validate a cookie against a user's session id
    /// </summary>
    /// <remarks>
    /// This uses another cookie to track the last checked time which is done for a few reasons:    
    /// * We can't use the user's auth ticket to do thsi because we'd be re-issuing the auth ticket all of the time and it would never expire
    ///     plus the auth ticket size is much larger than this small value
    /// * This will execute quite often (every minute per user) and in some cases there might be several requests that end up re-issuing the cookie so the cookie value should be small
    /// * We want to avoid the user lookup if it's not required so that will only happen when the time diff is great enough in the cookie
    /// </remarks>
    internal static class SessionIdValidator
    {
        public const string CookieName = "UMB_UCONTEXT_C";
        
        public static async Task ValidateSessionAsync(TimeSpan validateInterval, CookieValidateIdentityContext context)
        {
            if (context.Request.Uri.IsBackOfficeRequest(HttpRuntime.AppDomainAppVirtualPath) == false)
                return;
            
            var valid = await ValidateSessionAsync(validateInterval, context.OwinContext, context.Options.CookieManager, context.Options.SystemClock, context.Properties.IssuedUtc, context.Identity);

            if (valid == false)
            {
                context.RejectIdentity();
                context.OwinContext.Authentication.SignOut(context.Options.AuthenticationType);
            }
        }

        public static async Task<bool> ValidateSessionAsync(
            TimeSpan validateInterval,
            IOwinContext owinCtx,
            ICookieManager cookieManager,
            ISystemClock systemClock,
            DateTimeOffset? authTicketIssueDate,
            ClaimsIdentity currentIdentity)
        {
            if (owinCtx == null) throw new ArgumentNullException("owinCtx");
            if (cookieManager == null) throw new ArgumentNullException("cookieManager");
            if (systemClock == null) throw new ArgumentNullException("systemClock");

            DateTimeOffset? issuedUtc = null;
            var currentUtc = systemClock.UtcNow;

            //read the last checked time from a custom cookie
            var lastCheckedCookie = cookieManager.GetRequestCookie(owinCtx, CookieName);

            if (lastCheckedCookie.IsNullOrWhiteSpace() == false)
            {
                DateTimeOffset parsed;
                if (DateTimeOffset.TryParse(lastCheckedCookie, out parsed))
                {                    
                    issuedUtc = parsed;
                }
            }

            //no cookie, use the issue time of the auth ticket
            if (issuedUtc.HasValue == false)
            {
                issuedUtc = authTicketIssueDate;
            }

            // Only validate if enough time has elapsed
            var validate = issuedUtc.HasValue == false;
            if (issuedUtc.HasValue)
            {
                var timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                validate = timeElapsed > validateInterval;
            }

            if (validate == false)
                return true;

            var manager = owinCtx.GetUserManager<BackOfficeUserManager>();            
            if (manager == null)
                return false;

            var userId = currentIdentity.GetUserId<int>();
            var user = await manager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var sessionId = currentIdentity.FindFirstValue(Constants.Security.SessionIdClaimType);
            if (await manager.ValidateSessionIdAsync(userId, sessionId) == false)
                return false;

            //we will re-issue the cookie last checked cookie
            cookieManager.AppendResponseCookie(
                owinCtx,
                CookieName,
                DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"),
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = GlobalSettings.UseSSL || owinCtx.Request.IsSecure,
                    Path = "/"
                });

            return true;
        }
        
    }
}