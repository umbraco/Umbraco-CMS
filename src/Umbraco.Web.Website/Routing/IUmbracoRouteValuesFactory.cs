using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Umbraco.Web.Common.Routing;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Website.Routing
{
    /// <summary>
    /// Used to create <see cref="UmbracoRouteValues"/>
    /// </summary>
    public interface IUmbracoRouteValuesFactory
    {
        /// <summary>
        /// Creates <see cref="UmbracoRouteValues"/>
        /// </summary>
        UmbracoRouteValues Create(HttpContext httpContext, IPublishedRequest request);
    }
}
