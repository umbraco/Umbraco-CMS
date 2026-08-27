using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.SchemaLockdown;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.SchemaLockdown;

/// <summary>
/// Declares which schema entity type a controller manages, and denies any change to it the schema lockdown
/// restrictions do not permit.
/// </summary>
/// <remarks>
/// Declaring the entity type is all a controller has to do: the attribute is an authorization filter, and MVC
/// collects filters from base classes, so a newly added endpoint on a governed controller is covered without anyone
/// having to remember to tag it. Whether the entity type is restricted at all is decided solely by the registered
/// <see cref="ISchemaLockdownConfigurator"/> instances, never by this attribute.
/// <para>
/// Being an MVC filter rather than an authorization requirement is deliberate. Authorization requirements are all
/// evaluated together, so one cannot tell whether the others were satisfied; filters run only once the authorization
/// middleware has admitted the request, so a denial here means schema lockdown is the reason and nothing else was
/// also in the way.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class SchemaEntityTypeAttribute : Attribute, IAsyncAuthorizationFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaEntityTypeAttribute"/> class.
    /// </summary>
    /// <param name="entityType">The entity type the controller manages.</param>
    public SchemaEntityTypeAttribute(string entityType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        EntityType = entityType;
    }

    /// <summary>
    /// Gets the entity type the controller manages.
    /// </summary>
    public string EntityType { get; }

    /// <inheritdoc />
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        HttpContext httpContext = context.HttpContext;

        // A runtime that is not serving requests normally has to be left to the parts of the pipeline that answer
        // for it - denying here would mask their response.
        IRuntimeState runtimeState = httpContext.RequestServices.GetRequiredService<IRuntimeState>();
        if (runtimeState.Level != RuntimeLevel.Run && runtimeState.Level != RuntimeLevel.Upgrade)
        {
            return Task.CompletedTask;
        }

        SchemaOperation operation = ResolveOperation(httpContext, context.ActionDescriptor);

        ISchemaRestrictions restrictions =
            httpContext.RequestServices.GetRequiredService<ISchemaRestrictions>();
        if (restrictions.IsAllowed(EntityType, operation))
        {
            return Task.CompletedTask;
        }

        // The denial is an ordinary bodyless 403, as every other one in this API is: a 403 on these endpoints can
        // equally come from the permissions the request has already passed, and one status code cannot describe two
        // bodies. The header names what was denied for whoever is reading the network tab.
        httpContext.Response.Headers[Constants.Headers.SchemaLockdown] =
            $"{EntityType.ToLowerInvariant()}:{operation.ToString().ToLowerInvariant()}";

        context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);

        return Task.CompletedTask;
    }

    private static SchemaOperation ResolveOperation(HttpContext httpContext, ActionDescriptor actionDescriptor)
    {
        SchemaOperation? declared = (actionDescriptor as ControllerActionDescriptor)?.MethodInfo
            .GetCustomAttribute<SchemaOperationAttribute>()?.Operation;

        return declared ?? SchemaOperationResolver.Resolve(httpContext.Request.Method);
    }
}
