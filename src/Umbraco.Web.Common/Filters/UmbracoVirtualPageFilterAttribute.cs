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
        Endpoint? endpoint = context.HttpContext.GetEndpoint();

        // Check if there is any delegate in the metadata of the route, this
        // will occur when using the ForUmbraco method during routing.
        CustomRouteContentFinderDelegate? contentFinder =
            endpoint?.Metadata.OfType<CustomRouteContentFinderDelegate>().FirstOrDefault();

        if (contentFinder != null)
        {
            await SetUmbracoRouteValues(context, contentFinder.FindContent(context));
        }
        else
        {
            // Check if the controller is IVirtualPageController and then use that to FindContent
            if (context.Controller is IVirtualPageController ctrl)
            {
                await SetUmbracoRouteValues(context, ctrl.FindContent(context));
            }
        }

        // if we've assigned not found, just exit
        if (!(context.Result is NotFoundResult))
        {
            await next();
        }
    }

    private async Task SetUmbracoRouteValues(ActionExecutingContext context, IPublishedContent? content)
    {
        if (content != null)
        {
            UriUtility uriUtility = context.HttpContext.RequestServices.GetRequiredService<UriUtility>();

            var originalRequestUrl = new Uri(context.HttpContext.Request.GetEncodedUrl());
            Uri cleanedUrl = uriUtility.UriToUmbraco(originalRequestUrl);

            IPublishedRouter router = context.HttpContext.RequestServices.GetRequiredService<IPublishedRouter>();

            IPublishedRequestBuilder requestBuilder = await router.CreateRequestAsync(cleanedUrl);
            requestBuilder.SetPublishedContent(content);
            IPublishedRequest publishedRequest = requestBuilder.Build();

            var routeValues = new UmbracoRouteValues(
                publishedRequest,
                (ControllerActionDescriptor)context.ActionDescriptor);

            context.HttpContext.Features.Set(routeValues);
        }
        else
        {
            // if there is no content then it should be a not found
            context.Result = new NotFoundResult();
        }
    }
}
