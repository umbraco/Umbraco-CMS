using System;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
{
    public class BackOfficeCookieAuthenticationProvider : CookieAuthenticationProvider
    {
        public override void ResponseSignOut(CookieResponseSignOutContext context)
        {
            base.ResponseSignOut(context);

            //Make sure the definitely all of these cookies are cleared when signing out with cookies
            context.Response.Cookies.Append(UmbracoConfig.For.UmbracoSettings().Security.AuthCookieName, "", new CookieOptions
            {
                Expires = DateTime.Now.AddYears(-1),
                Path = "/"
            });
            context.Response.Cookies.Append(Constants.Web.PreviewCookieName, "", new CookieOptions
            {
                Expires = DateTime.Now.AddYears(-1),
                Path = "/"
            });
            context.Response.Cookies.Append(Constants.Security.BackOfficeExternalCookieName, "", new CookieOptions
            {
                Expires = DateTime.Now.AddYears(-1),
                Path = "/"
            });
        }

        /// <summary>
        /// Ensures that the culture is set correctly for the current back office user and that the user's session token is valid
        /// </summary>
        /// <param name="context"/>
        /// <returns/>
        public override Task ValidateIdentity(CookieValidateIdentityContext context)
        {
            EnsureCulture(context);

            return base.ValidateIdentity(context);
        }

        /// <summary>
        /// Ensures that the user has a valid session id
        /// </summary>
        /// <remarks>
        /// So that we are not overloading the database this throttles it's check to every minute
        /// </remarks>
        private void EnsureValidSessionId()
        {
            SessionIdValidator
                .OnValidateIdentity<BackOfficeUserManager, BackOfficeIdentityUser, int>(
                    TimeSpan.FromMinutes(1),
                    (manager, user) => user.GenerateUserIdentityAsync(manager),
                    identity => identity.GetUserId<int>());
        }

        private void EnsureCulture(CookieValidateIdentityContext context)
        {
            var umbIdentity = context.Identity as UmbracoBackOfficeIdentity;
            if (umbIdentity != null && umbIdentity.IsAuthenticated)
            {
                Thread.CurrentThread.CurrentCulture =
                    Thread.CurrentThread.CurrentUICulture =
                        new CultureInfo(umbIdentity.Culture);
            }
        }
    }

    /// <summary>
    /// Static helper class used to configure a CookieAuthenticationProvider 
    /// to validate a cookie against a user's session id
    /// </summary>
    /// <remarks>
    /// NOTE - this is borrowed from the SecurityStampValidator source since we want to do very similar things: 
    /// https://aspnetidentity.codeplex.com/SourceControl/latest#src/Microsoft.AspNet.Identity.Owin/SecurityStampValidator.cs
    /// </remarks>
    internal static class SessionIdValidator
    {
        /// <summary>
        /// Can be used as the ValidateIdentity method for a CookieAuthenticationProvider which will check a user's security
        /// stamp after validateInterval
        /// Rejects the identity if the stamp changes, and otherwise will call regenerateIdentity to sign in a new
        /// ClaimsIdentity
        /// </summary>
        /// <typeparam name="TManager"></typeparam>
        /// <typeparam name="TUser"></typeparam>
        /// <param name="validateInterval"></param>
        /// <param name="regenerateIdentity"></param>
        /// <returns></returns>
        public static Func<CookieValidateIdentityContext, Task> OnValidateIdentity<TManager, TUser>(
            TimeSpan validateInterval, Func<TManager, TUser, Task<ClaimsIdentity>> regenerateIdentity)
            where TManager : UserManager<TUser, string>
            where TUser : class, IUser<string>
        {
            return OnValidateIdentity(validateInterval, regenerateIdentity, id => id.GetUserId());
        }

        /// <summary>
        /// Can be used as the ValidateIdentity method for a CookieAuthenticationProvider which will check a user's security
        /// stamp after validateInterval
        /// Rejects the identity if the stamp changes, and otherwise will call regenerateIdentity to sign in a new
        /// ClaimsIdentity
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="validateInterval"></param>
        /// <param name="regenerateIdentityCallback"></param>
        /// <param name="getUserIdCallback"></param>
        /// <returns></returns>
        public static Func<CookieValidateIdentityContext, Task> OnValidateIdentity<TUser, TKey>(
            TimeSpan validateInterval,
            Func<BackOfficeUserManager<BackOfficeIdentityUser>, BackOfficeIdentityUser, Task<ClaimsIdentity>> regenerateIdentityCallback,
            Func<ClaimsIdentity, TKey> getUserIdCallback)
            
            where TUser : class, IUser<TKey>
            where TKey : IEquatable<TKey>
        {
            if (getUserIdCallback == null)
            {
                throw new ArgumentNullException("getUserIdCallback");
            }
            return async context =>
            {
                var currentUtc = DateTimeOffset.UtcNow;
                if (context.Options != null && context.Options.SystemClock != null)
                {
                    currentUtc = context.Options.SystemClock.UtcNow;
                }
                var issuedUtc = context.Properties.IssuedUtc;

                // Only validate if enough time has elapsed
                var validate = (issuedUtc == null);
                if (issuedUtc != null)
                {
                    var timeElapsed = currentUtc.Subtract(issuedUtc.Value);
                    validate = timeElapsed > validateInterval;
                }
                if (validate)
                {
                    var manager = context.OwinContext.GetUserManager<TManager>();
                    var userId = getUserIdCallback(context.Identity);
                    if (manager != null && userId != null)
                    {
                        var user = await manager.FindByIdAsync(userId);
                        var reject = true;
                        //TODO: if the sessionid doesn't match, then reject
                        if (user != null)
                        {
                            var securityStamp = context.Identity.FindFirstValue(DefaultSecurityStampClaimType);
                            if (securityStamp == await manager.GetSecurityStampAsync(userId))
                            {
                                reject = false;
                                // Regenerate fresh claims if possible and resign in
                                if (regenerateIdentityCallback != null)
                                {
                                    var identity = await regenerateIdentityCallback.Invoke(manager, user);
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