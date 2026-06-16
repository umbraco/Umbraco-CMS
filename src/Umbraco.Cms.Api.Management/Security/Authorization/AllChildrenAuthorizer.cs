using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization;

/// <summary>
/// Authorizes permissions on all direct children of a node.
/// </summary>
internal static class AllChildrenAuthorizer
{
    /// <summary>
    /// Determines whether the user is authorized for every direct child of the given parent (or the root).
    /// </summary>
    /// <param name="authorizationService">The authorization service.</param>
    /// <param name="entityService">The entity service used to resolve the children.</param>
    /// <param name="user">The current user.</param>
    /// <param name="parentKey">The parent key, or <c>null</c> to authorize the root-level children.</param>
    /// <param name="objectType">The object type of the children (and parent).</param>
    /// <param name="resourceFactory">Builds the permission resource to authorize a batch of child keys against.</param>
    /// <param name="policy">The authorization policy to apply.</param>
    /// <returns><c>true</c> if the user is authorized against all children; otherwise <c>false</c>.</returns>
    public static async Task<bool> IsAuthorizedForChildrenAsync(
        IAuthorizationService authorizationService,
        IEntityService entityService,
        ClaimsPrincipal user,
        Guid? parentKey,
        UmbracoObjectTypes objectType,
        Func<IEnumerable<Guid>, IPermissionResource> resourceFactory,
        string policy)
    {
        const int pageSize = 500;
        var page = 0;
        long total;
        do
        {
            Guid[] childKeys = entityService
                .GetPagedChildren(parentKey, [objectType], objectType, page * pageSize, pageSize, out total)
                .Select(child => child.Key)
                .ToArray();

            if (childKeys.Length > 0)
            {
                AuthorizationResult authorizationResult = await authorizationService.AuthorizeResourceAsync(
                    user,
                    resourceFactory(childKeys),
                    policy);

                if (authorizationResult.Succeeded is false)
                {
                    return false;
                }
            }

            page++;
        }
        while (page * pageSize < total);

        return true;
    }
}
