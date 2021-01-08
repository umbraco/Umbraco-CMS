using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Routing;
using Umbraco.Web.Common.Routing;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Common.Localization
{
    /// <summary>
    /// Sets the request culture to the culture of the <see cref="IPublishedRequest"/> if one is found in the request
    /// </summary>
    public class UmbracoPublishedContentCultureProvider : RequestCultureProvider
    {
        /// <inheritdoc/>
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext.GetRouteValue(Core.Constants.Web.UmbracoRouteDefinitionDataToken) is UmbracoRouteValues routeValues)
            {
                CultureInfo culture = routeValues.PublishedRequest?.Culture;
                if (culture != null)
                {
                    return Task.FromResult(new ProviderCultureResult(culture.Name));
                }
            }

            return NullProviderCultureResult;
        }
    }
}
