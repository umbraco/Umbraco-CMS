using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Extensions;

namespace Umbraco.Cms.Web.Common.Routing;

/// <summary>
/// This is used to setup the virtual page route so the route values and content are set for virtual pages.
/// </summary>
public class UmbracoVirtualPageRoute : IUmbracoVirtualPageRoute
{
    private readonly EndpointDataSource _endpointDataSource;
    private readonly LinkParser _linkParser;
    private readonly UriUtility _uriUtility;
    private readonly IPublishedRouter _publishedRouter;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="endpointDataSource">The endpoint data source.</param>
    /// <param name="linkParser">The link parser.</param>
    /// <param name="uriUtility">The Uri utility.</param>
    /// <param name="publishedRouter">The published router.</param>
    /// <param name="umbracoContextAccessor">The umbraco context accessor.</param>
    public UmbracoVirtualPageRoute(
        EndpointDataSource endpointDataSource,
        LinkParser linkParser,
        UriUtility uriUtility,
        IPublishedRouter publishedRouter,
        IUmbracoContextAccessor umbracoContextAccessor)
    {
        _endpointDataSource = endpointDataSource;
        _linkParser = linkParser;
        _uriUtility = uriUtility;
        _publishedRouter = publishedRouter;
        _umbracoContextAccessor = umbracoContextAccessor;
    }

    [Obsolete("Please use constructor that takes an IUmbracoContextAccessor instead, scheduled for removal in v17")]
    public UmbracoVirtualPageRoute(
        EndpointDataSource endpointDataSource,
        LinkParser linkParser,
        UriUtility uriUtility,
        IPublishedRouter publishedRouter)
        : this(endpointDataSource, linkParser, uriUtility, publishedRouter, StaticServiceProvider.Instance.GetRequiredService<IUmbracoContextAccessor>())
    {
    }

    /// <summary>
    /// This sets up the virtual page route for the current request if a mtahcing endpoint is found.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>Nothing</returns>
    public async Task SetupVirtualPageRoute(HttpContext httpContext)
    {
        // Try and find an endpoint for the current path...
        Endpoint? endpoint = _endpointDataSource.GetEndpointByPath(_linkParser, httpContext.Request.Path, out RouteValueDictionary? routeValues);

        if (endpoint != null && routeValues != null)
        {
            ControllerActionDescriptor? controllerActionDescriptor = endpoint.GetControllerActionDescriptor();

            if (controllerActionDescriptor != null)
            {
                Type controllerType = controllerActionDescriptor.ControllerTypeInfo.AsType();

                if (controllerType != null)
                {
                    // Get the controller for the endpoint. We need to fallback to ActivatorUtilities if the controller is not registered in DI.
                    var controller = httpContext.RequestServices.GetService(controllerType)
                                     ?? ActivatorUtilities.CreateInstance(httpContext.RequestServices, controllerType);

                    // Try and find the content if this is a virtual page
                    IPublishedContent? publishedContent = FindContent(
                        endpoint,
                        httpContext,
                        routeValues,
                        controllerActionDescriptor,
                        controller);

                    if (publishedContent != null)
                    {
                        // If we have content then set the route values
                        await SetRouteValues(httpContext, publishedContent, controllerActionDescriptor);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Finds the content from the custom route finder delegate or the virtual page controller.
    /// Note - This creates a dummay action executing context so the FindContent method of the
    /// IVirtualPageController can be called (without changing the interface contract).
    /// </summary>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="routeValues">The route values.</param>
    /// <param name="controllerActionDescriptor">The action descriptor.</param>
    /// <param name="controller">The controller.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Finds the content from the custom route finder delegate or the virtual page controller.
    /// </summary>
    /// <param name="endpoint">The endpoint.</param>
    /// <param name="actionExecutingContext">The action executing context.</param>
    /// <returns>The published content if found or null.</returns>
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

    /// <summary>
    /// Creates the published request for the published content.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="publishedContent">The published content.</param>
    /// <returns>The published request.</returns>
    public async Task<IPublishedRequest> CreatePublishedRequest(HttpContext httpContext, IPublishedContent publishedContent)
    {
        var originalRequestUrl = new Uri(httpContext.Request.GetEncodedUrl());
        Uri cleanedUrl = _uriUtility.UriToUmbraco(originalRequestUrl);

        IPublishedRequestBuilder requestBuilder = await _publishedRouter.CreateRequestAsync(cleanedUrl);
        requestBuilder.SetPublishedContent(publishedContent);
        _publishedRouter.RouteDomain(requestBuilder);

        // Ensure the culture and domain is set correctly for the published request
        return await _publishedRouter.RouteRequestAsync(requestBuilder, new RouteRequestOptions(Core.Routing.RouteDirection.Inbound));
    }

    /// <summary>
    /// Sets the route values for the published content and the controller action descriptor.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="publishedContent">The published content.</param>
    /// <param name="controllerActionDescriptor">The controller action descriptor.</param>
    /// <returns>Nothing.</returns>
    public async Task SetRouteValues(HttpContext httpContext, IPublishedContent publishedContent, ControllerActionDescriptor controllerActionDescriptor)
    {
        IPublishedRequest publishedRequest = await CreatePublishedRequest(httpContext, publishedContent);

        // Ensure the published request is set to the UmbracoContext
        if (_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext))
        {
            umbracoContext.PublishedRequest = publishedRequest;
        }

        var umbracoRouteValues = new UmbracoRouteValues(
            publishedRequest,
            controllerActionDescriptor);

        httpContext.Features.Set(umbracoRouteValues);
    }
}
