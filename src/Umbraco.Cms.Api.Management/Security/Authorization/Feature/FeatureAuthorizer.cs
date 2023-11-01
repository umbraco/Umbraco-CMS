using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Feature;

/// <inheritdoc />
internal sealed class FeatureAuthorizer : IFeatureAuthorizer
{
    private readonly IRuntimeState _runtimeState;
    private readonly UmbracoFeatures _umbracoFeatures;

    public FeatureAuthorizer(IRuntimeState runtimeState, UmbracoFeatures umbracoFeatures)
    {
        _runtimeState = runtimeState;
        _umbracoFeatures = umbracoFeatures;
    }

    /// <inheritdoc />
    public async Task<bool> IsAuthorizedAsync(AuthorizationHandlerContext context)
    {
        Endpoint? endpoint = null;

        if (_runtimeState.Level != RuntimeLevel.Run && _runtimeState.Level != RuntimeLevel.Upgrade)
        {
            return false;
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
        return await Task.FromResult(_umbracoFeatures.IsControllerEnabled(controllerType));
    }
}
