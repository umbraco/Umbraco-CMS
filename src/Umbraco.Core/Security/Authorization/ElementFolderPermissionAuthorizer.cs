using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <inheritdoc />
internal sealed class ElementFolderPermissionAuthorizer : IElementFolderPermissionAuthorizer
{
    private readonly IElementFolderPermissionService _elementFolderPermissionService;

    public ElementFolderPermissionAuthorizer(IElementFolderPermissionService elementFolderPermissionService) =>
        _elementFolderPermissionService = elementFolderPermissionService;

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAsync(
        IUser currentUser,
        IEnumerable<Guid> folderKeys,
        ISet<string> permissionsToCheck)
    {
        var folderKeyList = folderKeys.ToList();
        if (folderKeyList.Count == 0)
        {
            // Must succeed this requirement since we cannot process it.
            return true;
        }

        ElementAuthorizationStatus result =
            await _elementFolderPermissionService.AuthorizeAccessAsync(currentUser, folderKeyList, permissionsToCheck);

        // If we can't find the folder item(s) then we can't determine whether you are denied access.
        return result is not (ElementAuthorizationStatus.Success or ElementAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAtRootLevelAsync(IUser currentUser, ISet<string> permissionsToCheck)
    {
        ElementAuthorizationStatus result =
            await _elementFolderPermissionService.AuthorizeRootAccessAsync(currentUser, permissionsToCheck);

        // If we can't find the folder item(s) then we can't determine whether you are denied access.
        return result is not (ElementAuthorizationStatus.Success or ElementAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAtRecycleBinLevelAsync(IUser currentUser, ISet<string> permissionsToCheck)
    {
        ElementAuthorizationStatus result =
            await _elementFolderPermissionService.AuthorizeBinAccessAsync(currentUser, permissionsToCheck);

        // If we can't find the folder item(s) then we can't determine whether you are denied access.
        return result is not (ElementAuthorizationStatus.Success or ElementAuthorizationStatus.NotFound);
    }
}
