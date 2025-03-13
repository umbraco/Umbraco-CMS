using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Routing;

namespace Umbraco.Cms.Web.Common.Filters;

/// <summary>
///     Used to set the <see cref="UmbracoRouteValues" /> request feature based on the
///     <see cref="CustomRouteContentFinderDelegate" /> specified (if any)
///     for the custom route.
/// </summary>
public class UmbracoVirtualPageFilterAttribute : Attribute, IAsyncActionFilter
{
    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Check if there's proxied ViewData (i.e. returned from a SurfaceController)
        // We don't want to find the content and set route values again if this is from a surface controller
        ProxyViewDataFeature? proxyViewDataFeature = context.HttpContext.Features.Get<ProxyViewDataFeature>();

        if (proxyViewDataFeature != null)
        {
            if (context.Controller is Controller controller)
            {
                foreach (KeyValuePair<string, object?> kv in proxyViewDataFeature.ViewData)
                {
                    controller.ViewData[kv.Key] = kv.Value;
                }
            }
        }
        else
        {
            Endpoint? endpoint = context.HttpContext.GetEndpoint();

            if (endpoint != null)
            {
                IUmbracoVirtualPageRoute umbracoVirtualPageRoute = context.HttpContext.RequestServices.GetRequiredService<IUmbracoVirtualPageRoute>();
                IPublishedRouter publishedRouter = context.HttpContext.RequestServices.GetRequiredService<IPublishedRouter>();
                UriUtility uriUtility = context.HttpContext.RequestServices.GetRequiredService<UriUtility>();

                var originalRequestUrl = new Uri(context.HttpContext.Request.GetEncodedUrl());
                Uri cleanedUri = uriUtility.UriToUmbraco(originalRequestUrl);
                publishedRouter.UpdateVariationContext(cleanedUri);

                IPublishedContent? publishedContent = umbracoVirtualPageRoute.FindContent(endpoint, context);

                if (publishedContent != null)
                {
                    await umbracoVirtualPageRoute.SetRouteValues(
                        context.HttpContext,
                        publishedContent,
                        (ControllerActionDescriptor)context.ActionDescriptor);
                }
                else
                {
                    // if there is no content then it should be a not found
                    context.Result = new NotFoundResult();
                }
            }
        }

        // if we've assigned not found, just exit
        if (!(context.Result is NotFoundResult))
        {
            await next();
        }
    }
}
