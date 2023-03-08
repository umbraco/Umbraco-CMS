using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.Routing;

/// <summary>
/// Ensures the surface controller requests takes priority over other things like virtual routes.
/// Also ensures that requests to a surface controller on a virtual route will return 405, like HttpMethodMatcherPolicy ensures for non-virtual route requests.
/// </summary>
internal class SurfaceControllerMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
{
    private const string Http405EndpointDisplayName = "405 HTTP Method Not Supported";

    public override int Order { get; } // default order should be okay. Count be everything positive to not conflict with MS policies

    public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
    {
        // In theory all endpoints can have the query string data for a surface controller
        return true;
    }

    public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        if (candidates == null)
        {
            throw new ArgumentNullException(nameof(candidates));
        }

        if (candidates.Count < 2)
        {
            return Task.CompletedTask;
        }

        var surfaceControllerIndex = -1;
        var surfaceControllerAllowedHttpMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < candidates.Count; i++)
        {
            CandidateState candidate = candidates[i];

            ControllerActionDescriptor? controllerActionDescriptor =
                candidate.Endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (controllerActionDescriptor?.ControllerTypeInfo.IsType<SurfaceController>() == true)
            {
                surfaceControllerIndex = i;
                PopulateAllowedHttpMethods(candidate, surfaceControllerAllowedHttpMethods);
                break;
            }
        }

        if (surfaceControllerIndex >= 0)
        {
            if (surfaceControllerAllowedHttpMethods.Any()
                && surfaceControllerAllowedHttpMethods.Contains(httpContext.Request.Method) is false)
            {
                // We need to handle this as a 405 like the HttpMethodMatcherPolicy would do.
                httpContext.SetEndpoint(CreateRejectEndpoint(surfaceControllerAllowedHttpMethods));
                httpContext.Request.RouteValues = null!;
            }
            else
            {
                // Otherwise we invalidate all other endpoints than the surface controller that matched.
                InvalidateAllCandidatesExceptIndex(candidates, surfaceControllerIndex);
            }
        }

        return Task.CompletedTask;
    }

    private static void InvalidateAllCandidatesExceptIndex(CandidateSet candidates, int index)
    {
        for (var i = 0; i < candidates.Count; i++)
        {
            if (i != index)
            {
                candidates.SetValidity(i, false);
            }
        }
    }

    private static void PopulateAllowedHttpMethods(CandidateState candidate, ISet<string> surfaceControllerAllowedHttpMethods)
    {

        IHttpMethodMetadata? httpMethodMetadata = candidate.Endpoint?.Metadata.GetMetadata<IHttpMethodMetadata>();
        if (httpMethodMetadata is not null)
        {
            foreach (var httpMethod in httpMethodMetadata.HttpMethods)
            {
                surfaceControllerAllowedHttpMethods.Add(httpMethod);
            }
        }

    }

    private static Endpoint CreateRejectEndpoint(ISet<string> allowedHttpMethods) =>
        new Endpoint(
            (context) =>
            {
                context.Response.StatusCode = 405;

                context.Response.Headers.Allow = string.Join(", ", allowedHttpMethods);

                return Task.CompletedTask;
            },
            EndpointMetadataCollection.Empty,
            Http405EndpointDisplayName);
}
