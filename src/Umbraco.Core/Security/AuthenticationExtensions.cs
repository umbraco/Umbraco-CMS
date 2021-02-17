using System.Globalization;
using System.Security.Principal;
using System.Threading;
using Umbraco.Extensions;

namespace Umbraco.Core.Security
{
    public static class AuthenticationExtensions
    {
        /// <summary>
        /// Ensures that the thread culture is set based on the back office user's culture
        /// </summary>
        public static void EnsureCulture(this IIdentity identity)
        {
            var culture = GetCulture(identity);
            if (!(culture is null))
            {
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        public static CultureInfo GetCulture(this IIdentity identity)
        {
            if (identity is UmbracoBackOfficeIdentity umbIdentity && umbIdentity.IsAuthenticated)
            {
                return CultureInfo.GetCultureInfo(umbIdentity.GetCultureString());
            }

            return null;
        }
    }
}
