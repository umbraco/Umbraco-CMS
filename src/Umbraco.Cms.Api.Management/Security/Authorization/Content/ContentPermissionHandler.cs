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
            result &= await _contentPermissionAuthorizer.IsDeniedAtRootLevelAsync(context.User, resource.PermissionsToCheck) is false;
        }

        if (resource.CheckRecycleBin)
        {
            result &= await _contentPermissionAuthorizer.IsDeniedAtRecycleBinLevelAsync(context.User, resource.PermissionsToCheck) is false;
        }

        if (resource.ParentKeyForBranch is not null)
        {
            result &= await _contentPermissionAuthorizer.IsDeniedWithDescendantsAsync(context.User, resource.ParentKeyForBranch.Value, resource.PermissionsToCheck) is false;
        }

        if (resource.ContentKeys.Any())
        {
            result &= await _contentPermissionAuthorizer.IsDeniedAsync(context.User, resource.ContentKeys, resource.PermissionsToCheck) is false;
        }

        return result;
    }
}
