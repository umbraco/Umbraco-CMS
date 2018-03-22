using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Umbraco.Core.Configuration;
using Umbraco.Core.Services;

namespace Umbraco.Core.Security
{
    public class BackOfficeCookieAuthenticationProvider : CookieAuthenticationProvider
    {
        private readonly IUserService _userService;
        private readonly IRuntimeState _runtimeState;

        public BackOfficeCookieAuthenticationProvider(IUserService userService, IRuntimeState runtimeState)
        {
            _userService = userService;
            _runtimeState = runtimeState;
        }

        public override void ResponseSignIn(CookieResponseSignInContext context)
        {
            if (context.Identity is UmbracoBackOfficeIdentity backOfficeIdentity)
            {
                //generate a session id and assign it
                //create a session token - if we are configured and not in an upgrade state then use the db, otherwise just generate one

                var session = _runtimeState.Level == RuntimeLevel.Run
                    ? _userService.CreateLoginSession((int)backOfficeIdentity.Id, context.OwinContext.GetCurrentRequestIpAddress())
                    : Guid.NewGuid();

                backOfficeIdentity.UserData.SessionId = session.ToString();
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
                    _userService.ClearLoginSession(guidSession);
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
            EnsureCulture(context);

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
            if (_runtimeState.Level == RuntimeLevel.Run)
                await SessionIdValidator.ValidateSessionAsync(TimeSpan.FromMinutes(1), context);
        }

        private void EnsureCulture(CookieValidateIdentityContext context)
        {
            var umbIdentity = context.Identity as UmbracoBackOfficeIdentity;
            if (umbIdentity != null && umbIdentity.IsAuthenticated)
            {
                Thread.CurrentThread.CurrentCulture =
                    Thread.CurrentThread.CurrentUICulture =
                        UserCultures.GetOrAdd(umbIdentity.Culture, s => new CultureInfo(s));
            }
        }

        /// <summary>
        /// Used so that we aren't creating a new CultureInfo object for every single request
        /// </summary>
        private static readonly ConcurrentDictionary<string, CultureInfo> UserCultures = new ConcurrentDictionary<string, CultureInfo>();
    }
}
