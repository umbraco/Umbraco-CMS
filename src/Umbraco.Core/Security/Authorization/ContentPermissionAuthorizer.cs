using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <inheritdoc />
internal sealed class ContentPermissionAuthorizer : IContentPermissionAuthorizer
{
    private readonly IContentPermissionService _contentPermissionService;

    public ContentPermissionAuthorizer(IContentPermissionService contentPermissionService) =>
        _contentPermissionService = contentPermissionService;

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAsync(
        IUser currentUser,
        IEnumerable<Guid> contentKeys,
        ISet<string> permissionsToCheck)
    {
        var contentKeyList = contentKeys.ToList();
        if (contentKeyList.Count == 0)
        {
            // Must succeed this requirement since we cannot process it.
            return true;
        }

        ContentAuthorizationStatus result =
            await _contentPermissionService.AuthorizeAccessAsync(currentUser, contentKeyList, permissionsToCheck);

        // If we can't find the content item(s) then we can't determine whether you are denied access.
        return result is not (ContentAuthorizationStatus.Success or ContentAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedWithDescendantsAsync(
        IUser currentUser,
        Guid parentKey,
        ISet<string> permissionsToCheck)
    {
        ContentAuthorizationStatus result =
            await _contentPermissionService.AuthorizeDescendantsAccessAsync(currentUser, parentKey, permissionsToCheck);

        // If we can't find the content item(s) then we can't determine whether you are denied access.
        return result is not (ContentAuthorizationStatus.Success or ContentAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAtRootLevelAsync(IUser currentUser, ISet<string> permissionsToCheck)
    {
        ContentAuthorizationStatus result =
            await _contentPermissionService.AuthorizeRootAccessAsync(currentUser, permissionsToCheck);

        // If we can't find the content item(s) then we can't determine whether you are denied access.
        return result is not (ContentAuthorizationStatus.Success or ContentAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAtRecycleBinLevelAsync(IUser currentUser, ISet<string> permissionsToCheck)
    {
        ContentAuthorizationStatus result =
            await _contentPermissionService.AuthorizeBinAccessAsync(currentUser, permissionsToCheck);

        // If we can't find the content item(s) then we can't determine whether you are denied access.
        return result is not (ContentAuthorizationStatus.Success or ContentAuthorizationStatus.NotFound);
    }

    public async Task<bool> IsDeniedForCultures(IUser currentUser, ISet<string> culturesToCheck)
    {
        ContentAuthorizationStatus result =
            await _contentPermissionService.AuthorizeCultureAccessAsync(currentUser, culturesToCheck);

        // If we can't find the content item(s) then we can't determine whether you are denied access.
        return result is not (ContentAuthorizationStatus.Success or ContentAuthorizationStatus.NotFound);
    }
}
