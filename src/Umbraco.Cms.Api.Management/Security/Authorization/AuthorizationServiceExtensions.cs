using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Security.Authorization;

namespace Umbraco.Extensions;

public static class AuthorizationServiceExtensions
{
    public static Task<AuthorizationResult> AuthorizeResourceAsync(this IAuthorizationService authorizationService, ClaimsPrincipal user, IPermissionResource resource, string policyName)
        => authorizationService.AuthorizeAsync(user, resource, policyName);
}
