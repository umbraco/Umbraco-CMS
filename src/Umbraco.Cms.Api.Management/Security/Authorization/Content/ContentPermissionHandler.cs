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
        if (resource.CheckRoot)
        {
            return await _contentPermissionAuthorizer.IsAuthorizedAtRootLevelAsync(context.User, resource.PermissionsToCheck);
        }

        if (resource.CheckRecyleBin)
        {
            return await _contentPermissionAuthorizer.IsAuthorizedAtRecycleBinLevelAsync(context.User, resource.PermissionsToCheck);
        }

        if (resource.ParentKey is not null)
        {
            return await _contentPermissionAuthorizer.IsAuthorizedWithDescendantsAsync(context.User, resource.ParentKey.Value, resource.PermissionsToCheck);
        }

        return await _contentPermissionAuthorizer.IsAuthorizedAsync(context.User, resource.ContentKeys, resource.PermissionsToCheck);
    }
}
