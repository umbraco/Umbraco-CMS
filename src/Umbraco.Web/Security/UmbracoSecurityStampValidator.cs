using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Cookies;
using Umbraco.Core;
using Umbraco.Web.Models.Identity;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// Adapted from Microsoft.AspNet.Identity.Owin.SecurityStampValidator
    /// </summary>
    public class UmbracoSecurityStampValidator
    {
        public static Func<CookieValidateIdentityContext, Task> OnValidateIdentity<TSignInManager, TManager, TUser>(
            TimeSpan validateInterval,
            Func<BackOfficeSignInManager, TManager, TUser, Task<ClaimsIdentity>> regenerateIdentityCallback,
            Func<ClaimsIdentity, string> getUserIdCallback)
            where TSignInManager : BackOfficeSignInManager
            where TManager : BackOfficeUserManager<TUser>
            where TUser : BackOfficeIdentityUser
        {
            if (getUserIdCallback == null) throw new ArgumentNullException(nameof(getUserIdCallback));

            return async context =>
            {
                var currentUtc = DateTimeOffset.UtcNow;
                if (context.Options != null && context.Options.SystemClock != null)
                {
                    currentUtc = context.Options.SystemClock.UtcNow;
                }

                var issuedUtc = context.Properties.IssuedUtc;

                // Only validate if enough time has elapsed
                var validate = issuedUtc == null;
                if (issuedUtc != null)
                {
                    var timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                    validate = timeElapsed > validateInterval;
                }

                if (validate)
                {
                    var manager = context.OwinContext.Get<TManager>();
                    if (manager == null) throw new InvalidOperationException("Unable to load BackOfficeUserManager");

                    var signInManager = context.OwinContext.Get<TSignInManager>();
                    if (signInManager == null) throw new InvalidOperationException("Unable to load BackOfficeSignInManager");

                    var userId = getUserIdCallback(context.Identity);

                    if (userId != null)
                    {
                        var user = await manager.FindByIdAsync(userId);
                        var reject = true;

                        // Refresh the identity if the stamp matches, otherwise reject
                        if (user != null && manager.SupportsUserSecurityStamp)
                        {
                            var securityStamp = context.Identity.FindFirst(Constants.Web.SecurityStampClaimType)?.Value;
                            var newSecurityStamp = await manager.GetSecurityStampAsync(user);

                            if (securityStamp == newSecurityStamp)
                            {
                                reject = false;
                                // Regenerate fresh claims if possible and resign in
                                if (regenerateIdentityCallback != null)
                                {
                                    var identity = await regenerateIdentityCallback.Invoke(signInManager, manager, user);
                                    if (identity != null)
                                    {
                                        // Fix for regression where this value is not updated
                                        // Setting it to null so that it is refreshed by the cookie middleware
                                        context.Properties.IssuedUtc = null;
                                        context.Properties.ExpiresUtc = null;
                                        context.OwinContext.Authentication.SignIn(context.Properties, identity);
                                    }
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
            };
        }
    }
}
