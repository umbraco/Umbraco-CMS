using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Validation.AspNetCore;
using Umbraco.Cms.Api.Management.Security.Authorization;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Membership.Permissions;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.WIP;

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
        PermissionMatchingBehaviour = WIP.PermissionMatchingBehaviour.All;
    }

    public ContextualAuthorizeAttribute(
        IEnumerable<string> permissions,
        string context,
        PermissionMatchingBehaviour permissionMatchingBehaviour)
    {
        Permissions = permissions;
        Context = context;
        PermissionMatchingBehaviour = permissionMatchingBehaviour;
    }

    private IEnumerable<string> Permissions { get; }

    private string Context { get; }

    private PermissionMatchingBehaviour PermissionMatchingBehaviour { get; }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

        AuthorizationResult authorizationResult = await authorizationService.AuthorizeResourceAsync(
            context.HttpContext.User,
            ContextualPermissionResource.WithSetup(Permissions, Context, PermissionMatchingBehaviour),
            ContextualPermissionHandler.ContextualPermissionsPolicyAlias);

        if (authorizationResult.Succeeded is false)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.HttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>()!.ReasonPhrase =
                "Invalid contextual permission";
            context.Result = new JsonResult("Invalid contextual permission")
            {
                Value = new { Status = "Unauthorized", Message = "Invalid contextual permission" }
            };
        }
    }
}

/// <summary>
/// Used in combination with <see cref="ContextualPermissionRequirement"/> by <see cref="ContextualPermissionHandler"/>
/// </summary>
public class ContextualPermissionResource : IPermissionResource
{
    public static ContextualPermissionResource WithPermission(string permission, string context) =>
        WithAllPermissions(permission.Yield(), context);

    public static ContextualPermissionResource WithAnyPermissions(IEnumerable<string> permissions, string context) =>
        new(permissions, context, PermissionMatchingBehaviour.Any);

    public static ContextualPermissionResource WithAllPermissions(IEnumerable<string> permissions, string context) =>
        new(permissions, context, PermissionMatchingBehaviour.All);

    public static ContextualPermissionResource WithSetup(
        IEnumerable<string> permissions,
        string context,
        PermissionMatchingBehaviour permissionMatchingBehaviour) =>
        new(permissions, context, permissionMatchingBehaviour);

    private ContextualPermissionResource(
        IEnumerable<string> permissions,
        string context,
        PermissionMatchingBehaviour behaviour)
    {
        Permissions = permissions;
        Context = context;
        PermissionMatchingBehaviour = behaviour;
    }

    public IEnumerable<string> Permissions { get; }

    public string Context { get; }

    public PermissionMatchingBehaviour PermissionMatchingBehaviour { get; }
}

public enum PermissionMatchingBehaviour
{
    Any,
    All
}

/// <summary>
/// Used in combination with <see cref="ContextualPermissionResource"/> by <see cref="ContextualPermissionHandler"/>
/// This requirement only has per request parameters, see <see cref="ContextualPermissionResource"/>
/// </summary>
public class ContextualPermissionRequirement : IAuthorizationRequirement
{
}

/// Authorizes a certain permission (read,write,browse,...) within a given context (umbraco, my-package,...)
/// against the <see cref="IGranularPermission">granular permissions</see> defined on all <see cref="IUserGroup">user groups</see> the <see cref="IUser">user</see> is part off.
/// Permission
public class ContextualPermissionHandler : MustSatisfyRequirementAuthorizationHandler<ContextualPermissionRequirement,
    ContextualPermissionResource>
{
    public const string ContextualPermissionsPolicyAlias = "Umbraco.ContextualPermissions";
    private readonly IAuthorizationHelper _authorizationHelper;

    public ContextualPermissionHandler(IAuthorizationHelper authorizationHelper)
    {
        _authorizationHelper = authorizationHelper;
    }

    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        ContextualPermissionRequirement requirement,
        ContextualPermissionResource resource)
    {
        IUser user = _authorizationHelper.GetUmbracoUser(context.User);

        return resource.PermissionMatchingBehaviour == PermissionMatchingBehaviour.Any
            ? resource.Permissions.Any(ContextualMatch)
            : resource.Permissions.All(ContextualMatch);

        bool ContextualMatch(string permissionToCheck) =>
            user.Groups.SelectMany(g => g.GranularPermissions)
                .Any(definedContextualPermission =>
                    definedContextualPermission.Context.Equals(resource.Context, StringComparison.InvariantCultureIgnoreCase)
                    && definedContextualPermission.Permission.Equals(permissionToCheck, StringComparison.InvariantCultureIgnoreCase));
    }
}

public class ContextualPermissionPolicyComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IAuthorizationHandler, ContextualPermissionHandler>();
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(ContextualPermissionHandler.ContextualPermissionsPolicyAlias, policy =>
            {
                policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
                policy.Requirements.Add(new ContextualPermissionRequirement());
            });
    }
}
