using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Api.Management.Security.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
///     Authorizes that the schema lockdown rules permit the requested change to schema.
/// </summary>
internal sealed class SchemaLockdownAuthorizationHandler : MustSatisfyRequirementAuthorizationHandler<EntityTypeAttribute>
{
    private readonly SchemaLockdownRules _rules;
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaLockdownAuthorizationHandler"/> class.
    /// </summary>
    /// <param name="rules">The frozen decision table.</param>
    /// <param name="runtimeState">The runtime state.</param>
    public SchemaLockdownAuthorizationHandler(
        SchemaLockdownRules rules,
        IRuntimeState runtimeState)
    {
        _rules = rules;
        _runtimeState = runtimeState;
    }

    /// <inheritdoc />
    protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, EntityTypeAttribute requirement)
    {
        // Authorization runs ahead of every MVC filter, so a runtime that is not serving requests normally has to be
        // left to the parts of the pipeline that answer for it - denying here would mask their response.
        if (_runtimeState.Level != RuntimeLevel.Run && _runtimeState.Level != RuntimeLevel.Upgrade)
        {
            return Task.FromResult(true);
        }

        SchemaOperation operation = ResolveOperation(context.Resource);

        return Task.FromResult(_rules.IsAllowed(requirement.EntityType, operation));
    }

    private static SchemaOperation ResolveOperation(object? resource)
    {
        HttpContext? httpContext = resource switch
        {
            HttpContext resourceHttpContext => resourceHttpContext,
            AuthorizationFilterContext authorizationFilterContext => authorizationFilterContext.HttpContext,
            _ => null,
        };

        SchemaOperation? declared = httpContext?.GetEndpoint()?.Metadata
            .GetMetadata<ControllerActionDescriptor>()?.MethodInfo
            .GetCustomAttribute<SchemaOperationAttribute>()?.Operation;

        // An authorization call carrying no request has no verb to infer from, which the resolver reports as an
        // unclassified operation rather than as a read.
        return SchemaOperationResolver.Resolve(httpContext?.Request.Method, declared);
    }
}
