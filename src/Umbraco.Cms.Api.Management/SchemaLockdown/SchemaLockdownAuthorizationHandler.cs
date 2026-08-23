using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Api.Management.Security.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
///     Authorizes that the current configuration permits the requested change to schema.
/// </summary>
internal sealed class SchemaLockdownAuthorizationHandler : MustSatisfyRequirementAuthorizationHandler<SchemaLockdownEntityTypeRequirement>
{
    private readonly ISchemaLockdownMatrixAccessor _matrixAccessor;
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaLockdownAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="matrixAccessor">Provides the frozen decision matrix.</param>
    /// <param name="runtimeState">The runtime state.</param>
    public SchemaLockdownAuthorizationHandler(
        ISchemaLockdownMatrixAccessor matrixAccessor,
        IRuntimeState runtimeState)
    {
        _matrixAccessor = matrixAccessor;
        _runtimeState = runtimeState;
    }

    /// <inheritdoc />
    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, SchemaLockdownEntityTypeRequirement requirement)
    {
        // Authorization runs ahead of every MVC filter, so a runtime that is not serving requests normally has to be
        // left to the parts of the pipeline that answer for it - denying here would mask their response.
        if (_runtimeState.Level != RuntimeLevel.Run && _runtimeState.Level != RuntimeLevel.Upgrade)
        {
            return Task.FromResult(true);
        }

        SchemaOperation operation = ResolveOperation(context.Resource);

        if (operation == SchemaOperation.Read)
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(_matrixAccessor.Matrix.IsAllowed(requirement.EntityType, operation));
    }

    private static SchemaOperation ResolveOperation(object? resource)
    {
        HttpContext? httpContext = null;
        Endpoint? endpoint = null;

        switch (resource)
        {
            case DefaultHttpContext defaultHttpContext:
                httpContext = defaultHttpContext;
                break;

            case AuthorizationFilterContext authorizationFilterContext:
                httpContext = authorizationFilterContext.HttpContext;
                break;

            case Endpoint resourceEndpoint:
                endpoint = resourceEndpoint;
                break;
        }

        endpoint ??= httpContext?.Features.Get<IEndpointFeature>()?.Endpoint;

        SchemaOperationAttribute? declared = endpoint?.Metadata
            .GetMetadata<ControllerActionDescriptor>()?.MethodInfo
            .GetCustomAttribute<SchemaOperationAttribute>();

        // An authorization call carrying no request has no verb to infer from, which the resolver reports as an
        // unclassified operation rather than as a read.
        string[] httpMethods = httpContext is null ? [] : [httpContext.Request.Method];

        return SchemaOperationResolver.Resolve(httpMethods, declared);
    }
}
