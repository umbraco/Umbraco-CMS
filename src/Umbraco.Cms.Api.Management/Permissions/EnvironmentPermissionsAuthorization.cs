using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Permissions;

namespace Umbraco.Cms.Api.Management.Permissions;

/// <summary>
/// Denies a request that needs a permission verb the environment permissions deny.
/// </summary>
internal static class EnvironmentPermissionsAuthorization
{
    /// <summary>
    /// Forbids the request when any permission verb it needs is denied.
    /// </summary>
    /// <param name="context">The authorization filter context.</param>
    /// <param name="resolvePermissions">Resolves the permission verbs the request needs. Only called when something is denied.</param>
    public static void Authorize(AuthorizationFilterContext context, Func<IEnumerable<string>> resolvePermissions)
    {
        HttpContext httpContext = context.HttpContext;
        IEnvironmentPermissions environmentPermissions =
            httpContext.RequestServices.GetRequiredService<IEnvironmentPermissions>();

        if (environmentPermissions.DeniedPermissions.Count == 0)
        {
            return;
        }

        var denied = resolvePermissions().FirstOrDefault(environmentPermissions.IsDenied);
        if (denied is null)
        {
            return;
        }

        // A bodyless 403 like every other one in this API, as permissions can return a 403 on the same endpoints. The
        // header names the denied permission verb. It's returned at every runtime level, including during an upgrade,
        // where the maintenance mode filter would otherwise answer 503 ("try again later").
        httpContext.Response.Headers[Constants.Headers.DeniedPermission] = denied;

        context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
    }
}
