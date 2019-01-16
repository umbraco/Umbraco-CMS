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
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Security;
using Umbraco.Core.Services;

namespace Umbraco.Web.Security
{
    public class BackOfficeCookieAuthenticationProvider : CookieAuthenticationProvider
    {
        private readonly IUserService _userService;
        private readonly IRuntimeState _runtimeState;
        private readonly IGlobalSettings _globalSettings;

        public BackOfficeCookieAuthenticationProvider(IUserService userService, IRuntimeState runtimeState, IGlobalSettings globalSettings)
        {
            _userService = userService;
            _runtimeState = runtimeState;
            _globalSettings = globalSettings;
        }

        public override void ResponseSignIn(CookieResponseSignInContext context)
        {
            if (context.Identity is UmbracoBackOfficeIdentity backOfficeIdentity)
            {
                //generate a session id and assign it
                //create a session token - if we are configured and not in an upgrade state then use the db, otherwise just generate one

                var session = _runtimeState.Level == RuntimeLevel.Run
                    ? _userService.CreateLoginSession(backOfficeIdentity.Id, context.OwinContext.GetCurrentRequestIpAddress())
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
                var sessionId = claimsIdentity.FindFirstValue(Core.Constants.Security.SessionIdClaimType);
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
            context.Response.Cookies.Append(Current.Configs.Settings().Security.AuthCookieName, "", new CookieOptions
            {
                Expires = DateTime.Now.AddYears(-1),
                Path = "/"
            });
            context.Response.Cookies.Append(Core.Constants.Web.PreviewCookieName, "", new CookieOptions
            {
                Expires = DateTime.Now.AddYears(-1),
                Path = "/"
            });
            context.Response.Cookies.Append(Core.Constants.Security.BackOfficeExternalCookieName, "", new CookieOptions
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
        protected virtual async Task EnsureValidSessionId(CookieValidateIdentityContext context)
        {
            if (_runtimeState.Level == RuntimeLevel.Run)
                await SessionIdValidator.ValidateSessionAsync(TimeSpan.FromMinutes(1), context, _globalSettings);
        }




    }
}
