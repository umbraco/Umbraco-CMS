using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
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

        public static readonly object CookieLocker = new object();

        public static async Task ValidateSession(TimeSpan validateInterval, CookieValidateIdentityContext context)
        {
            if (context.Request.Uri.IsBackOfficeRequest(HttpRuntime.AppDomainAppVirtualPath) == false)
                return;

            DateTimeOffset? issuedUtc = null;
            var currentUtc = DateTimeOffset.UtcNow;

            //read the last checked time from a custom cookie
            var lastCheckedCookie = context.Options.CookieManager.GetRequestCookie(context.OwinContext, CookieName);
            
            if (lastCheckedCookie.IsNullOrWhiteSpace() == false)
            {
                DateTimeOffset parsed;
                if (DateTimeOffset.TryParse(lastCheckedCookie, out parsed))
                    issuedUtc = parsed;
            }

            //no cookie, use the issue time of the auth ticket
            if (issuedUtc.HasValue == false)
            {
                if (context.Options != null && context.Options.SystemClock != null)
                {
                    currentUtc = context.Options.SystemClock.UtcNow;
                }
                issuedUtc = context.Properties.IssuedUtc;
            }

            // Only validate if enough time has elapsed
            var validate = (issuedUtc == null);
            if (issuedUtc != null)
            {
                var timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                validate = timeElapsed > validateInterval;
            }
            if (validate)
            {
                var manager = context.OwinContext.GetUserManager<BackOfficeUserManager>();
                var userId = context.Identity.GetUserId<int>();
                if (manager != null)
                {
                    var user = await manager.FindByIdAsync(userId);
                    var reject = true;                    
                    if (user != null)
                    {
                        //get or create the session checker for the user, this allows us to perform some locking and to check the server side
                        //last checked date/time which allows us to avoid a bunch of requests being checked at the same time and thus a bunch of
                        //db calls to check the session id at the same time and also writing back cookies for each request. Essentially this just
                        //makes things perform  better to reduce the amount of db calls and cookie writing.
                        var sessionChecker = SessionChecks.GetOrAdd(user.Id, i => new SessionCheck
                        {
                            LastChecked = issuedUtc ?? DateTimeOffset.UtcNow,
                            //1,1 means only one thread at a time
                            Locker = new SemaphoreSlim(1, 1)
                        });

                        //re-evaluate
                        if (issuedUtc != null)
                        {
                            var timeElapsed = currentUtc.Subtract(sessionChecker.LastChecked);
                            validate = timeElapsed > validateInterval;
                        }
                        
                        //immediately update the last checked date which will update it in our SessionChecks memory storage
                        //so that subsequent requests occuring at the same time don't perform the check
                        sessionChecker.LastChecked = DateTimeOffset.UtcNow;

                        //we use a lock per user to prevent having the same cookie being written multiple times if multiple requests come in at the very same
                        //time. This has occured for me when clicking on a node that has multiple pickers and multiple REST calls are made at the same time this
                        //validation occurs. This would mean that we would make more db lookups than required and also write the cookie out more times than required
                        //so this prevents this scenario.
                        if (validate)
                        {                            
                            try
                            {
                                await sessionChecker.Locker.WaitAsync().ConfigureAwait(false);

                                //re-evaluate inside the lock
                                var timeElapsed = currentUtc.Subtract(sessionChecker.LastChecked);
                                validate = timeElapsed > validateInterval;
                                if (validate == false)
                                {
                                    //exit, another thread must have beat us to it
                                    reject = false;
                                }
                                else
                                {
                                    var sessionId = context.Identity.FindFirstValue(Constants.Security.SessionIdClaimType);
                                    if (await manager.ValidateSessionIdAsync(userId, sessionId))
                                    {
                                        reject = false;

                                        //we will re-issue the cookie last checked cookie
                                        context.Options.CookieManager.AppendResponseCookie(
                                            context.OwinContext,
                                            CookieName,
                                            DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"),
                                            new CookieOptions
                                            {
                                                HttpOnly = true,
                                                Secure = GlobalSettings.UseSSL || context.Request.IsSecure,
                                                Path = "/"
                                            });
                                    }
                                }
                            }
                            finally
                            {
                                sessionChecker.Locker.Release();
                            }
                        }
                    }
                    if (reject)
                    {
                        context.RejectIdentity();
                        context.OwinContext.Authentication.SignOut(context.Options.AuthenticationType);
                    }
                }
            }
        }

        /// <summary>
        /// stores a last checked intance per user
        /// </summary>
        private static readonly ConcurrentDictionary<int, SessionCheck> SessionChecks = new ConcurrentDictionary<int, SessionCheck>();

        /// <summary>
        /// Used to store a locking object and a last checked date/time per user
        /// </summary>
        private struct SessionCheck
        {
            public SemaphoreSlim Locker { get; set; }
            public DateTimeOffset LastChecked { get; set; }
        }
    }
}