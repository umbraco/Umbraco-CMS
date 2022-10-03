using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Extensions;

namespace Umbraco.Cms.Web.Common.Routing;

public class UmbracoVirtualPageRoute : IUmbracoVirtualPageRoute
{
    private readonly EndpointDataSource _endpointDataSource;
    private readonly LinkParser _linkParser;
    private readonly UriUtility _uriUtility;
    private readonly IPublishedRouter _publishedRouter;

    public UmbracoVirtualPageRoute(
        EndpointDataSource endpointDataSource,
        LinkParser linkParser,
        UriUtility uriUtility,
        IPublishedRouter publishedRouter)
    {
        _endpointDataSource = endpointDataSource;
        _linkParser = linkParser;
        _uriUtility = uriUtility;
        _publishedRouter = publishedRouter;
    }

    public async Task SetupVirtualPageRoute(HttpContext httpContext)
    {
        Endpoint? endpoint = _endpointDataSource.GetEndpointByPath(_linkParser, httpContext.Request.Path, out RouteValueDictionary? routeValues);

        if (endpoint != null && routeValues != null)
        {
            ControllerActionDescriptor? controllerActionDescriptor = endpoint.GetControllerActionDescriptor();

            if (controllerActionDescriptor != null)
            {
                Type controllerType = controllerActionDescriptor.ControllerTypeInfo.AsType();

                if (controllerType != null)
                {
                    var controller = httpContext.RequestServices.GetRequiredService(controllerType);

                    IPublishedContent? publishedContent = FindContent(
                        endpoint,
                        httpContext,
                        routeValues,
                        controllerActionDescriptor,
                        controller);

                    if (publishedContent != null)
                    {
                        await SetRouteValues(httpContext, publishedContent, controllerActionDescriptor);
                    }
                }
            }
        }
    }

    public IPublishedContent? FindContent(
        Endpoint endpoint,
        HttpContext httpContext,
        RouteValueDictionary routeValues,
        ControllerActionDescriptor controllerActionDescriptor,
        object controller)
    {
        var actionExecutingContext = new ActionExecutingContext(
            new ActionContext(
                httpContext,
                new RouteData(routeValues),
                controllerActionDescriptor),
            filters: new List<IFilterMetadata>(),
            actionArguments: new Dictionary<string, object?>(),
            controller: controller);

        return FindContent(endpoint, actionExecutingContext);
    }

    public IPublishedContent? FindContent(Endpoint endpoint, ActionExecutingContext actionExecutingContext)
    {
        // Check if there is any delegate in the metadata of the route, this
        // will occur when using the ForUmbraco method during routing.
        CustomRouteContentFinderDelegate? contentFinder =
            endpoint?.Metadata.OfType<CustomRouteContentFinderDelegate>().FirstOrDefault();

        if (contentFinder != null)
        {
            return contentFinder.FindContent(actionExecutingContext);
        }
        else
        {
            // Check if the controller is IVirtualPageController and then use that to FindContent
            if (actionExecutingContext.Controller is IVirtualPageController virtualPageController)
            {
                return virtualPageController.FindContent(actionExecutingContext);
            }
        }

        return null;
    }

    public async Task<IPublishedRequest> CreatePublishedRequest(HttpContext httpContext, IPublishedContent publishedContent)
    {
        var originalRequestUrl = new Uri(httpContext.Request.GetEncodedUrl());
        Uri cleanedUrl = _uriUtility.UriToUmbraco(originalRequestUrl);

        IPublishedRequestBuilder requestBuilder = await _publishedRouter.CreateRequestAsync(cleanedUrl);
        requestBuilder.SetPublishedContent(publishedContent);

        return requestBuilder.Build();
    }

    public async Task SetRouteValues(HttpContext httpContext, IPublishedContent publishedContent, ControllerActionDescriptor controllerActionDescriptor)
    {
        IPublishedRequest publishedRequest = await CreatePublishedRequest(httpContext, publishedContent);

        var umbracoRouteValues = new UmbracoRouteValues(
            publishedRequest,
            controllerActionDescriptor);

        httpContext.Features.Set(umbracoRouteValues);
    }
}
