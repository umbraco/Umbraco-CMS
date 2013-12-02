using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Helpers;
using Umbraco.Core;

namespace Umbraco.Web.WebApi.Filters
{
    public static class AngularAntiForgeryHelper
    {
        public const string CookieName = "XSRF-TOKEN";
        public const string Headername = "X-XSRF-TOKEN";

        public static bool Validate(HttpRequestHeaders requestHeaders, out string failedReason)
        {
            failedReason = "";

            if (requestHeaders.Any(z => StringExtensions.InvariantEquals(z.Key, Headername)) == false)
            {
                failedReason = "Missing token";
                return false;
            }

            var headerToken = requestHeaders
                .Where(z => z.Key.InvariantEquals(Headername))
                .Select(z => z.Value)
                .SelectMany(z => z)
                .FirstOrDefault();

            var cookieToken = requestHeaders
                .GetCookies()
                .Select(c => c[CookieName])
                .FirstOrDefault();

            // both header and cookie must be there
            if (cookieToken == null || headerToken == null)
            {
                failedReason = "Missing token null";
                return false;
            }

            // ensure that the cookie matches the header and then ensure it matches the correct value!
            try
            {
                AntiForgery.Validate(cookieToken.Value, headerToken);
            }
            catch
            {
                failedReason = "Invalid token";
                return false;
            }

            return true;
        }
    }
}