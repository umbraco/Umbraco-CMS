using System;
using System.Globalization;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Umbraco.Core.Configuration;

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
        /// Ensures that the culture is set correctly for the current back office user
        /// </summary>
        /// <param name="context"/>
        /// <returns/>
        public override Task ValidateIdentity(CookieValidateIdentityContext context)
        {
            var umbIdentity = context.Identity as UmbracoBackOfficeIdentity;
            if (umbIdentity != null && umbIdentity.IsAuthenticated)
            {
                Thread.CurrentThread.CurrentCulture =
                    Thread.CurrentThread.CurrentUICulture =
                        new CultureInfo(umbIdentity.Culture);
            }

            return base.ValidateIdentity(context);
        }
    }
}