using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Umbraco.Core.Security;

namespace Umbraco.Web.Common.Localization
{

    /// <summary>
    /// Sets the request culture to the culture of the back office user if one is determined to be in the request
    /// </summary>
    public class UmbracoBackOfficeIdentityCultureProvider : RequestCultureProvider
    {
        /// <inheritdoc/>
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            CultureInfo culture = httpContext.User.Identity.GetCulture();

            if (culture is null)
            {
                return NullProviderCultureResult;
            }

            return Task.FromResult(new ProviderCultureResult(culture.Name));
        }
    }
}
