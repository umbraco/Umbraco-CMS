using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Contextual;

/// <summary>
/// Authorizes a certain permission (read,write,browse,...) within a given context (umbraco, my-package,...)
/// against the <see cref="IGranularPermission"> granular permissions</see> defined on all <see cref="IUserGroup">user groups</see> the <see cref="IUser">user</see> is part off.
/// This is accomplished by validating the <see cref="ContextualPermissionHandler.ContextualPermissionsPolicyAlias">Contextual Permissions Policy</see> trough the <see cref="IAuthorizationService"/>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ContextualAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    public ContextualAuthorizeAttribute(string permission, string context)
    {
        Permissions = permission.Yield();
        Context = context;
        PermissionMatchingBehaviour = PermissionMatchingBehaviour.All;
        PermissionScopeMatchingBehaviour = PermissionScopeMatchingBehaviour.Any;
    }

    public ContextualAuthorizeAttribute(
        IEnumerable<string> permissions,
        string context,
        PermissionMatchingBehaviour permissionMatchingBehaviour,
        PermissionScopeMatchingBehaviour permissionScopeMatchingBehaviour)
    {
        Permissions = permissions;
        Context = context;
        PermissionMatchingBehaviour = permissionMatchingBehaviour;
        PermissionScopeMatchingBehaviour = permissionScopeMatchingBehaviour;
    }

    private IEnumerable<string> Permissions { get; }

    private string Context { get; }

    private PermissionMatchingBehaviour PermissionMatchingBehaviour { get; }

    private PermissionScopeMatchingBehaviour PermissionScopeMatchingBehaviour { get; }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        IAuthorizationService authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

        AuthorizationResult authorizationResult = await authorizationService.AuthorizeResourceAsync(
            context.HttpContext.User,
            ContextualPermissionResource.WithSetup(Permissions, Context, PermissionMatchingBehaviour, PermissionScopeMatchingBehaviour),
            ContextualPermissionHandler.ContextualPermissionsPolicyAlias);

        if (authorizationResult.Succeeded is false)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.HttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>()!.ReasonPhrase =
                "Invalid contextual permission";
            context.Result = new JsonResult("Invalid contextual permission")
            {
                Value = new { Status = "Unauthorized", Message = "Invalid contextual permission" },
            };
        }
    }
}
