using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Web.Website.Routing;

/// <summary>
///     Used to create <see cref="UmbracoRouteValues" />
/// </summary>
public interface IUmbracoRouteValuesFactory
{
    /// <summary>
    ///     Creates <see cref="UmbracoRouteValues" />
    /// </summary>
    Task<UmbracoRouteValues> CreateAsync(HttpContext httpContext, IPublishedRequest request);
}
