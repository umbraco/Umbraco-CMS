using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Umbraco.Core.Security;

namespace Umbraco.Web.Common.Extensions
{
    public class UmbracoBackOfficeIdentityCultureProvider : RequestCultureProvider
    {
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            var culture = httpContext.User.Identity.GetCulture();

            if (culture is null)
            {
                return NullProviderCultureResult;
            }

            return Task.FromResult(new ProviderCultureResult(culture.Name, culture.Name));
        }
    }
}
