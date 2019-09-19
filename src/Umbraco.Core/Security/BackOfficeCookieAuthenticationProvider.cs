using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Semver;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Identity;

namespace Umbraco.Core.Security
{
    public class BackOfficeCookieAuthenticationProvider : CookieAuthenticationProvider
    {
        private readonly ApplicationContext _appCtx;

        [Obsolete("Use the ctor specifying all dependencies")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public BackOfficeCookieAuthenticationProvider()
            : this(ApplicationContext.Current)
        {
        }

        public BackOfficeCookieAuthenticationProvider(ApplicationContext appCtx)
        {
            if (appCtx == null) throw new ArgumentNullException("appCtx");
            _appCtx = appCtx;
        }

        private static readonly SemVersion MinUmbracoVersionSupportingLoginSessions = new SemVersion(7, 8);

        public override void ResponseSignIn(CookieResponseSignInContext context)
        {
            var backOfficeIdentity = context.Identity as UmbracoBackOfficeIdentity;
            if (backOfficeIdentity != null)
            {
                //generate a session id and assign it
                //create a session token - if we are configured and not in an upgrade state then use the db, otherwise just generate one

                //NOTE - special check because when we are upgrading to 7.8 we cannot create a session since the db isn't ready and we'll get exceptions
                var canAcquireSession = _appCtx.IsUpgrading == false || _appCtx.CurrentVersion() >= MinUmbracoVersionSupportingLoginSessions;

                var session = canAcquireSession
                    ? _appCtx.Services.UserService.CreateLoginSession((int)backOfficeIdentity.Id, context.OwinContext.GetCurrentRequestIpAddress())
                    : Guid.NewGuid();

                backOfficeIdentity.UserData.SessionId = session.ToString();
            }            

            base.ResponseSignIn(context);
        }

        public override void ResponseSignOut(CookieResponseSignOutContext context)
        {
            //Clear the user's session on sign out
            if (context != null && context.OwinContext != null && context.OwinContext.Authentication != null
                && context.OwinContext.Authentication.User != null && context.OwinContext.Authentication.User.Identity != null)
            {
                var claimsIdentity = context.OwinContext.Authentication.User.Identity as ClaimsIdentity;
                var sessionId = claimsIdentity.FindFirstValue(Constants.Security.SessionIdClaimType);
                Guid guidSession;
                if (sessionId.IsNullOrWhiteSpace() == false && Guid.TryParse(sessionId, out guidSession))
                {
                    _appCtx.Services.UserService.ClearLoginSession(guidSession);
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

            if (context?.Identity == null)
            {
                context?.OwinContext.Authentication.SignOut(context.Options.AuthenticationType);
                return;
            }
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
            if (_appCtx.IsConfigured && _appCtx.IsUpgrading == false)
                await SessionIdValidator.ValidateSessionAsync(TimeSpan.FromMinutes(1), context);
        }
    }
}
