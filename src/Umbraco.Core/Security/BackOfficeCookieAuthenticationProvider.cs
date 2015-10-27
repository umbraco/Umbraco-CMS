using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Cookies;

namespace Umbraco.Core.Security
{
    public class BackOfficeCookieAuthenticationProvider : CookieAuthenticationProvider
    {
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