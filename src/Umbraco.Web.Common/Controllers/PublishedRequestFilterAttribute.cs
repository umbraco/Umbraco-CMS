using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Web.Common.Controllers;

/// <summary>
///     Deals with custom headers for the umbraco request
/// </summary>
internal class PublishedRequestFilterAttribute : ResultFilterAttribute
{
    /// <summary>
    ///     Deals with custom headers for the umbraco request
    /// </summary>
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        UmbracoRouteValues routeVals = GetUmbracoRouteValues(context);
        IPublishedRequest pcr = routeVals.PublishedRequest;

        // now we can deal with headers, etc...
        if (pcr.ResponseStatusCode.HasValue)
        {
            // set status code -- even for redirects
            context.HttpContext.Response.StatusCode = pcr.ResponseStatusCode.Value;
        }

        AddCacheControlHeaders(context, pcr);

        if (pcr.Headers != null)
        {
            foreach (KeyValuePair<string, string> header in pcr.Headers)
            {
                context.HttpContext.Response.Headers.Append(header.Key, header.Value);
            }
        }
    }

    /// <summary>
    ///     Gets the <see cref="UmbracoRouteValues" />
    /// </summary>
    protected UmbracoRouteValues GetUmbracoRouteValues(ResultExecutingContext context)
    {
        UmbracoRouteValues? routeVals = context.HttpContext.Features.Get<UmbracoRouteValues>();
        if (routeVals == null)
        {
            throw new InvalidOperationException(
                $"No {nameof(UmbracoRouteValues)} feature was found in the HttpContext");
        }

        return routeVals;
    }

    private void AddCacheControlHeaders(ResultExecutingContext context, IPublishedRequest pcr)
    {
        var cacheControlHeaders = new List<string>();

        if (pcr.SetNoCacheHeader)
        {
            cacheControlHeaders.Add("no-cache");
        }

        if (pcr.CacheExtensions != null)
        {
            foreach (var cacheExtension in pcr.CacheExtensions)
            {
                cacheControlHeaders.Add(cacheExtension);
            }
        }

        if (cacheControlHeaders.Count > 0)
        {
            context.HttpContext.Response.Headers["Cache-Control"] = string.Join(", ", cacheControlHeaders);
        }
    }
}
