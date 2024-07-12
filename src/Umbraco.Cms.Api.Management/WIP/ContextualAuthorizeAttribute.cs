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
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.WIP;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ContextualAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    public ContextualAuthorizeAttribute(string verb, string context)
    {
        Verbs = verb.Yield();
        Context = context;
        VerbMatchingBehaviour = VerbMatchingBehaviour.All;
    }

    public ContextualAuthorizeAttribute(
        IEnumerable<string> verbs,
        string context,
        VerbMatchingBehaviour verbMatchingBehaviour)
    {
        Verbs = verbs;
        Context = context;
        VerbMatchingBehaviour = verbMatchingBehaviour;
    }

    public IEnumerable<string> Verbs { get; }
    public string Context { get; }
    public VerbMatchingBehaviour VerbMatchingBehaviour { get; }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();

        AuthorizationResult authorizationResult = await authorizationService.AuthorizeResourceAsync(
            context.HttpContext.User,
            ContextualPermissionResource.WithSetup(Verbs, Context, VerbMatchingBehaviour),
            ContextualPermissionHandler.ContextualPermissionsPolicy);

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

// define a resource that can have values per invocation of a authorization requirement
public class ContextualPermissionResource : IPermissionResource
{
    public static ContextualPermissionResource WithPermission(string verb, string context) =>
        WithAllPermissions(verb.Yield(), context);

    public static ContextualPermissionResource WithAnyPermissions(IEnumerable<string> verbs, string context) =>
        new(verbs, context, VerbMatchingBehaviour.Any);

    public static ContextualPermissionResource WithAllPermissions(IEnumerable<string> verbs, string context) =>
        new(verbs, context, VerbMatchingBehaviour.All);

    public static ContextualPermissionResource WithSetup(IEnumerable<string> verbs, string context,
        VerbMatchingBehaviour verbMatchingBehaviour) =>
        new(verbs, context, verbMatchingBehaviour);

    private ContextualPermissionResource(IEnumerable<string> verbs, string context, VerbMatchingBehaviour behaviour)
    {
        Verbs = verbs;
        Context = context;
        VerbMatchingBehaviour = behaviour;
    }

    public IEnumerable<string> Verbs { get; }
    public string Context { get; }
    public VerbMatchingBehaviour VerbMatchingBehaviour { get; }
}

public enum VerbMatchingBehaviour
{
    Any,
    All
}

// define the requirement
public class ContextualPermissionRequirement : IAuthorizationRequirement
{
    // since we handle all our parameters on a case by case basis, we place them in the PermissionResource which will be filled by the atrribute
}

// define handlers that match the resource value to the requirement, the base class implicitly fails/passes the requirement based on the IsAuthorized method
public class ContextualPermissionHandler : MustSatisfyRequirementAuthorizationHandler<ContextualPermissionRequirement,
    ContextualPermissionResource>
{
    public const string ContextualPermissionsPolicy = "Our.Umbraco.ContextualPermissions";
    private readonly IAuthorizationHelper _authorizationHelper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // dependency injection is available here, so get whatever you need
    public ContextualPermissionHandler(IAuthorizationHelper authorizationHelper, IHttpContextAccessor httpContextAccessor)
    {
        _authorizationHelper = authorizationHelper;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        ContextualPermissionRequirement requirement,
        ContextualPermissionResource resource)
    {
        var user = _authorizationHelper.GetUmbracoUser(context.User);

        return resource.VerbMatchingBehaviour == VerbMatchingBehaviour.Any
            ? resource.Verbs.Any(ContextualMatch)
            : resource.Verbs.All(ContextualMatch);

        bool ContextualMatch(string v) =>
            user.Groups.SelectMany(g => g.GranularPermissions)
                .Any(p => p.Context == resource.Context &&
                          p.Permission.Equals(v, StringComparison.InvariantCultureIgnoreCase));
    }
}

public class ContextPermissionComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IAuthorizationHandler, ContextualPermissionHandler>();
        builder.Services.AddAuthorization((options) =>
        {
            options.AddPolicy(ContextualPermissionHandler.ContextualPermissionsPolicy, policy =>
            {
                policy.AuthenticationSchemes.Add(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
                policy.Requirements.Add(new ContextualPermissionRequirement());
            });
        });
    }
}
