using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Security.Authorization;

namespace Umbraco.Extensions;

/// <summary>
/// Contains extension methods for working with authorization services in Umbraco.
/// </summary>
public static class AuthorizationServiceExtensions
{
    /// <summary>
    /// Asynchronously authorizes a resource using the specified authorization service, user, resource, and policy name.
    /// </summary>
    /// <param name="authorizationService">The authorization service to use for authorization.</param>
    /// <param name="user">The claims principal representing the user to authorize.</param>
    /// <param name="resource">The resource to authorize.</param>
    /// <param name="policyName">The name of the authorization policy to apply.</param>
    /// <returns>A task that represents the asynchronous authorization operation. The task result contains the authorization result.</returns>
    public static Task<AuthorizationResult> AuthorizeResourceAsync(this IAuthorizationService authorizationService, ClaimsPrincipal user, IPermissionResource resource, string policyName)
        => authorizationService.AuthorizeAsync(user, resource, policyName);
}
