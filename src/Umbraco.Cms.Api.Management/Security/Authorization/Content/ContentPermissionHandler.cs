using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Content;

/// <summary>
///     Authorizes that the current user has the correct permission access to the content item(s) specified in the request.
/// </summary>
public class ContentPermissionHandler : MustSatisfyRequirementAuthorizationHandler<ContentPermissionRequirement, ContentPermissionResource>
{
    private readonly IContentPermissionAuthorizer _contentPermissionAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionHandler" /> class.
    /// </summary>
    /// <param name="contentPermissionAuthorizer">Authorizer for content access.</param>
    public ContentPermissionHandler(IContentPermissionAuthorizer contentPermissionAuthorizer)
        => _contentPermissionAuthorizer = contentPermissionAuthorizer;

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(
        AuthorizationHandlerContext context,
        ContentPermissionRequirement requirement,
        ContentPermissionResource resource)
    {
        var result = true;

        if (resource.CheckRoot)
        {
            result &= await _contentPermissionAuthorizer.IsAuthorizedAtRootLevelAsync(context.User, resource.PermissionsToCheck);
        }

        if (resource.CheckRecycleBin)
        {
            result &= await _contentPermissionAuthorizer.IsAuthorizedAtRecycleBinLevelAsync(context.User, resource.PermissionsToCheck);
        }

        if (resource.ParentKeyForBranch is not null)
        {
            result &= await _contentPermissionAuthorizer.IsAuthorizedWithDescendantsAsync(context.User, resource.ParentKeyForBranch.Value, resource.PermissionsToCheck);
        }

        if (resource.ContentKeys.Any())
        {
            result &= await _contentPermissionAuthorizer.IsAuthorizedAsync(context.User, resource.ContentKeys, resource.PermissionsToCheck);
        }

        if (resource.CulturesToCheck is not null)
        {
            result &= await _contentPermissionAuthorizer.IsAuthorizedForCultures(context.User, resource.CulturesToCheck);
        }

        return result;
    }
}
