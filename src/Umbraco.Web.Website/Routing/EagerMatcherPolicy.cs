﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Routing;


/**
 * A matcher policy that discards the catch-all (slug) route if there are any other valid routes with a lower order.
 *
 * The purpose of this is to skip our expensive <see cref="UmbracoRouteValueTransformer"/> if it's not required,
 * for instance if there's a statically routed endpoint registered before the dynamic route,
 * for more information see: https://github.com/umbraco/Umbraco-CMS/issues/16015.
 * The core reason why this is necessary is that ALL routes get evaluated:
 * "
 * all routes get evaluated, they get to produce candidates and then the best candidate is selected.
 * Since you have a dynamic route, it needs to run to produce the final endpoints and
 * then those are ranked in along with the rest of the candidates to choose the final endpoint.
 * "
 * From: https://github.com/dotnet/aspnetcore/issues/45175#issuecomment-1322497958
 *
 * This also handles rerouting under install/upgrade states.
 */

internal class EagerMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
{
    private readonly IRuntimeState _runtimeState;
    private readonly EndpointDataSource _endpointDataSource;
    private readonly UmbracoRequestPaths _umbracoRequestPaths;
    private GlobalSettings _globalSettings;
    private readonly Lazy<Endpoint> _installEndpoint;
    private readonly Lazy<Endpoint> _renderEndpoint;

    public EagerMatcherPolicy(
        IRuntimeState runtimeState,
        EndpointDataSource endpointDataSource,
        UmbracoRequestPaths umbracoRequestPaths,
        IOptionsMonitor<GlobalSettings> globalSettings)
    {
        _runtimeState = runtimeState;
        _endpointDataSource = endpointDataSource;
        _umbracoRequestPaths = umbracoRequestPaths;
        _globalSettings = globalSettings.CurrentValue;
        globalSettings.OnChange(settings => _globalSettings = settings);
        _installEndpoint = new Lazy<Endpoint>(GetInstallEndpoint);
        _renderEndpoint = new Lazy<Endpoint>(GetRenderEndpoint);
    }

    // We want this to run as the very first policy, so we can discard the UmbracoRouteValueTransformer before the framework runs it.
    public override int Order => int.MinValue + 10;

    // We know we don't have to run this matcher against the backoffice endpoints.
    public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints) => true;

    public async Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            var handled = await HandleInstallUpgrade(httpContext, candidates);
            if (handled)
            {
                return;
            }
        }

        // If there's only one candidate, we don't need to do anything.
        if (candidates.Count < 2)
        {
            return;
        }

        // If there are multiple candidates, we want to discard the catch-all (slug)
        // IF there is any candidates with a lower order. Since this will be a statically routed endpoint registered before the dynamic route.
        // Which means that we don't have to run our UmbracoRouteValueTransformer to route dynamically (expensive).
        var lowestOrder = int.MaxValue;
        int? dynamicId = null;
        RouteEndpoint? dynamicEndpoint = null;
        for (var i = 0; i < candidates.Count; i++)
        {
            CandidateState candidate = candidates[i];

            // If it's not a RouteEndpoint there's not much we can do to count it in the order.
            if (candidate.Endpoint is not RouteEndpoint routeEndpoint)
            {
                continue;
            }

            if (routeEndpoint.Order < lowestOrder)
            {
                // We have to ensure that the route is valid for the current request method.
                // This is because attribute routing will always have an order of 0.
                // This means that you could attribute route a POST to /example, but also have an umbraco page at /example
                // This would then result in a 404, because we'd see the attribute route with order 0, and always consider that the lowest order
                // We'd then disable the dynamic endpoint since another endpoint has a lower order, and end up with only 1 invalid endpoint.
                // (IsValidCandidate does not take this into account since the candidate itself is still valid)
                HttpMethodMetadata? methodMetaData = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>();
                if (methodMetaData?.HttpMethods.Contains(httpContext.Request.Method) is false)
                {
                    continue;
                }

                lowestOrder = routeEndpoint.Order;
            }

            // We only want to consider our dynamic route, this way it's still possible to register your own custom route before ours.
            if (routeEndpoint.DisplayName != Constants.Web.Routing.DynamicRoutePattern)
            {
                continue;
            }

            dynamicEndpoint = routeEndpoint;
            dynamicId = i;
        }

        // Invalidate the dynamic route if another route has a lower order.
        // This means that if you register your static route after the dynamic route, the dynamic route will take precedence
        // This more closely resembles the existing behaviour.
        if (dynamicEndpoint is not null && dynamicId is not null && dynamicEndpoint.Order > lowestOrder)
        {
            candidates.SetValidity(dynamicId.Value, false);
        }
    }

    /// <summary>
    /// Replaces the first endpoint candidate with the specified endpoint, invalidating all other candidates,
    /// guaranteeing that the specified endpoint will be hit.
    /// </summary>
    /// <param name="candidates">The candidate set to manipulate.</param>
    /// <param name="endpoint">The target endpoint that will be hit.</param>
    /// <param name="routeValueDictionary"></param>
    private static void SetEndpoint(CandidateSet candidates, Endpoint endpoint, RouteValueDictionary routeValueDictionary)
    {
        candidates.ReplaceEndpoint(0, endpoint, routeValueDictionary);

        for (int i = 1; i < candidates.Count; i++)
        {
            candidates.SetValidity(1, false);
        }
    }

    private Endpoint GetInstallEndpoint()
    {
        Endpoint endpoint = _endpointDataSource.Endpoints.First(x =>
        {
            ControllerActionDescriptor? descriptor = x.Metadata.GetMetadata<ControllerActionDescriptor>();
            return descriptor?.ControllerTypeInfo.Name == "InstallController"
                   && descriptor.ActionName == "Index";
        });

        return endpoint;
    }

    private Endpoint GetRenderEndpoint()
    {
        Endpoint endpoint = _endpointDataSource.Endpoints.First(x =>
        {
            ControllerActionDescriptor? descriptor = x.Metadata.GetMetadata<ControllerActionDescriptor>();
            return descriptor?.ControllerTypeInfo == typeof(RenderController)
                   && descriptor.ActionName == nameof(RenderController.Index);
        });

        return endpoint;
    }

    private Task<bool> HandleInstallUpgrade(HttpContext httpContext, CandidateSet candidates)
    {
        if (_runtimeState.Level != RuntimeLevel.Upgrade)
        {
            // We need to let the installer API requests through
            // Currently we do this with a check for the installer path
            // Ideally we should do this in a more robust way, for instance with a dedicated attribute we can then check for.
            if (_umbracoRequestPaths.IsInstallerRequest(httpContext.Request.Path))
            {
                return Task.FromResult(true);
            }

            SetEndpoint(candidates, _installEndpoint.Value, new RouteValueDictionary
            {
                [Constants.Web.Routing.ControllerToken] = "Install",
                [Constants.Web.Routing.ActionToken] = "Index",
                [Constants.Web.Routing.AreaToken] = Constants.Web.Mvc.InstallArea,
            });

            return Task.FromResult(true);
        }

        // Check if maintenance page should be shown
        // Current behaviour is that statically routed endpoints still work in upgrade state
        // This means that IF there is a static route, we should not show the maintenance page.
        // And instead carry on as we normally would.
        var hasStaticRoute = false;
        for (var i = 0; i < candidates.Count; i++)
        {
            CandidateState candidate = candidates[i];
            IDynamicEndpointMetadata? dynamicEndpointMetadata = candidate.Endpoint.Metadata.GetMetadata<IDynamicEndpointMetadata>();
            if (dynamicEndpointMetadata is null || dynamicEndpointMetadata.IsDynamic is false)
            {
                hasStaticRoute = true;
                break;
            }
        }

        if (_runtimeState.Level != RuntimeLevel.Upgrade
            || _globalSettings.ShowMaintenancePageWhenInUpgradeState is false
            || hasStaticRoute)
        {
            return Task.FromResult(false);
        }

        // Otherwise we'll re-route to the render controller (this will in turn show the maintenance page through a filter)
        // With this approach however this could really just be a plain old endpoint instead of a filter.
        SetEndpoint(candidates, _renderEndpoint.Value, new RouteValueDictionary
        {
            [Constants.Web.Routing.ControllerToken] = ControllerExtensions.GetControllerName<RenderController>(),
            [Constants.Web.Routing.ActionToken] = nameof(RenderController.Index),
        });

        return Task.FromResult(true);

    }
}
