using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
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

    /// <summary>
    /// This sets up the virtual page route for the current request if a mtahcing endpoint is found.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>Nothing</returns>
    public async Task SetupVirtualPageRoute(HttpContext httpContext)
    {
        // This runs for every surface controller POST. If the request already routed to a real, non-404
        // content item then it is a normal content page - not a virtual page - so there is nothing to set up
        // and we can skip the endpoint lookup below. Note a virtual page URL typically routes to the
        // configured 404 content (non-null PublishedContent), so we must also check it is not a 404.
        if (_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext)
            && umbracoContext.PublishedRequest is { } publishedRequest
            && publishedRequest.HasPublishedContent()
            && publishedRequest.Is404() is false)
        {
            return;
        }

        // Try and find an endpoint for the current path. The name-based lookup only matches named routes
        // (e.g. custom routes registered via ForUmbracoPage), so fall back to matching by route pattern,
        // which also finds attribute-routed IVirtualPageController endpoints that have no route name (#14165).
        Endpoint? endpoint = _endpointDataSource.GetEndpointByPath(_linkParser, httpContext.Request.Path, out RouteValueDictionary? routeValues);

        if (endpoint is null || IsVirtualPageEndpoint(endpoint) is false)
        {
            endpoint = _endpointDataSource.GetEndpointByRoutePattern(httpContext.Request.Path, IsVirtualPageEndpoint, out routeValues);
        }

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
    /// Determines whether an endpoint is a virtual page endpoint: one whose content is resolved either by an
    /// <see cref="IVirtualPageController" /> or by a delegate registered via <c>ForUmbracoPage</c>
    /// (a <see cref="CustomRouteContentFinderDelegate" />). Mirrors the two mechanisms handled by
    /// <see cref="FindContent(Endpoint, ActionExecutingContext)" />.
    /// </summary>
    internal static bool IsVirtualPageEndpoint(Endpoint endpoint)
    {
        ControllerActionDescriptor? controllerActionDescriptor = endpoint.GetControllerActionDescriptor();
        if (controllerActionDescriptor is null)
        {
            return false;
        }

        return endpoint.Metadata.OfType<CustomRouteContentFinderDelegate>().Any()
               || typeof(IVirtualPageController).IsAssignableFrom(controllerActionDescriptor.ControllerTypeInfo);
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
    /// <remarks>
    /// The action arguments on the created context are populated from the raw route values (strings) and are
    /// not model-bound/type-converted as they would be by MVC. A FindContent implementation should therefore
    /// treat a route argument as a string (e.g. <c>sku is string s</c>) rather than expecting a converted type.
    /// </remarks>
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
            actionArguments: BuildActionArguments(routeValues, controllerActionDescriptor),
            controller: controller);

        return FindContent(endpoint, actionExecutingContext);
    }

    // Populates the action arguments from the matched route values, keyed by the action's parameter names.
    // The real MVC pipeline model-binds these for us, but this dummy context is built outside of it (on a
    // surface controller POST), so a FindContent that reads ActionArguments - as the documented example does -
    // would otherwise see nothing.
    // Note: values are the raw route values (strings); they are NOT type-converted the way MVC model binding
    // would (e.g. "42" stays a string, it is not converted to an int parameter's value).
    private static IDictionary<string, object?> BuildActionArguments(
        RouteValueDictionary routeValues,
        ControllerActionDescriptor controllerActionDescriptor)
    {
        var actionArguments = new Dictionary<string, object?>();

        if (controllerActionDescriptor.MethodInfo is not null)
        {
            foreach (var parameter in controllerActionDescriptor.MethodInfo.GetParameters())
            {
                if (parameter.Name is not null && routeValues.TryGetValue(parameter.Name, out var value))
                {
                    actionArguments[parameter.Name] = value;
                }
            }
        }

        return actionArguments;
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
