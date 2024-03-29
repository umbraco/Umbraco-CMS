using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Feature;

/// <summary>
///     Authorizes that the controller is an authorized Umbraco feature.
/// </summary>
public class FeatureAuthorizeHandler : MustSatisfyRequirementAuthorizationHandler<FeatureAuthorizeRequirement>
{
    private readonly IFeatureAuthorizer _featureAuthorizer;
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FeatureAuthorizeHandler" /> class.
    /// </summary>
    /// <param name="featureAuthorizer">Authorizer for Umbraco features.</param>
    /// <param name="runtimeState">The runtime state.</param>
    public FeatureAuthorizeHandler(IFeatureAuthorizer featureAuthorizer, IRuntimeState runtimeState)
    {
        _featureAuthorizer = featureAuthorizer;
        _runtimeState = runtimeState;
    }

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(AuthorizationHandlerContext context, FeatureAuthorizeRequirement requirement)
    {
        Endpoint? endpoint = null;

        if (_runtimeState.Level != RuntimeLevel.Run && _runtimeState.Level != RuntimeLevel.Upgrade)
        {
            return true;
        }

        switch (context.Resource)
        {
            case DefaultHttpContext defaultHttpContext:
            {
                IEndpointFeature? endpointFeature = defaultHttpContext.Features.Get<IEndpointFeature>();
                endpoint = endpointFeature?.Endpoint;
                break;
            }

            case AuthorizationFilterContext authorizationFilterContext:
            {
                IEndpointFeature? endpointFeature =
                    authorizationFilterContext.HttpContext.Features.Get<IEndpointFeature>();
                endpoint = endpointFeature?.Endpoint;
                break;
            }

            case Endpoint resourceEndpoint:
            {
                endpoint = resourceEndpoint;
                break;
            }
        }

        if (endpoint is null)
        {
            throw new InvalidOperationException("This authorization handler can only be applied to controllers routed with endpoint routing.");
        }

        ControllerActionDescriptor? actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
        Type? controllerType = actionDescriptor?.ControllerTypeInfo.AsType();
        if (controllerType is null)
        {
            return true;
        }

        return await _featureAuthorizer.IsDeniedAsync(controllerType) is false;
    }
}
