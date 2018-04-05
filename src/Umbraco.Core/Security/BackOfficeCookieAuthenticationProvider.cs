using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Services;

namespace Umbraco.Core.Security
{
    public class BackOfficeCookieAuthenticationProvider : CookieAuthenticationProvider
    {
        // fixme inject
        private IUserService UserService => Current.Services.UserService;
        private IRuntimeState RuntimeState => Current.RuntimeState;

        public override void ResponseSignIn(CookieResponseSignInContext context)
        {
            if (context.Identity is UmbracoBackOfficeIdentity backOfficeIdentity)
            {
                //generate a session id and assign it
                //create a session token - if we are configured and not in an upgrade state then use the db, otherwise just generate one

                var session = RuntimeState.Level == RuntimeLevel.Run
                    ? UserService.CreateLoginSession(backOfficeIdentity.Id, context.OwinContext.GetCurrentRequestIpAddress())
                    : Guid.NewGuid();

                backOfficeIdentity.SessionId = session.ToString();
            }

            base.ResponseSignIn(context);
        }

        public override void ResponseSignOut(CookieResponseSignOutContext context)
        {
            //Clear the user's session on sign out
            if (context?.OwinContext?.Authentication?.User?.Identity != null)
            {
                var claimsIdentity = context.OwinContext.Authentication.User.Identity as ClaimsIdentity;
                var sessionId = claimsIdentity.FindFirstValue(Constants.Security.SessionIdClaimType);
                if (sessionId.IsNullOrWhiteSpace() == false && Guid.TryParse(sessionId, out var guidSession))
                {
                    UserService.ClearLoginSession(guidSession);
                }
            }

            base.ResponseSignOut(context);

            //Make sure the definitely all of these cookies are cleared when signing out with cookies
            context.Response.Cookies.Append(SessionIdValidator.CookieName, "", new CookieOptions
            {
                Expires = DateTime.Now.AddYears(-1),
                Path = "/"
            });
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
        public override async Task ValidateIdentity(CookieValidateIdentityContext context)
        {
            //ensure the thread culture is set
            context?.Identity?.EnsureCulture();

            await EnsureValidSessionId(context);

            await base.ValidateIdentity(context);
        }

        /// <summary>
        /// Ensures that the user has a valid session id
        /// </summary>
        /// <remarks>
        /// So that we are not overloading the database this throttles it's check to every minute
        /// </remarks>
        protected  virtual async Task EnsureValidSessionId(CookieValidateIdentityContext context)
        {
            if (RuntimeState.Level == RuntimeLevel.Run)
                await SessionIdValidator.ValidateSessionAsync(TimeSpan.FromMinutes(1), context);
        }

        

        
    }
}
