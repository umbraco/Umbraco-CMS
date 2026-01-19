using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <inheritdoc />
internal sealed class ElementPermissionAuthorizer : IElementPermissionAuthorizer
{
    private readonly IElementPermissionService _elementPermissionService;

    public ElementPermissionAuthorizer(IElementPermissionService elementPermissionService) =>
        _elementPermissionService = elementPermissionService;

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAsync(
        IUser currentUser,
        IEnumerable<Guid> elementKeys,
        ISet<string> permissionsToCheck)
    {
        var elementKeyList = elementKeys.ToList();
        if (elementKeyList.Count == 0)
        {
            // Must succeed this requirement since we cannot process it.
            return true;
        }

        ElementAuthorizationStatus result =
            await _elementPermissionService.AuthorizeAccessAsync(currentUser, elementKeyList, permissionsToCheck);

        // If we can't find the element item(s) then we can't determine whether you are denied access.
        return result is not (ElementAuthorizationStatus.Success or ElementAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedWithDescendantsAsync(
        IUser currentUser,
        Guid parentKey,
        ISet<string> permissionsToCheck)
    {
        ElementAuthorizationStatus result =
            await _elementPermissionService.AuthorizeDescendantsAccessAsync(currentUser, parentKey, permissionsToCheck);

        // If we can't find the element item(s) then we can't determine whether you are denied access.
        return result is not (ElementAuthorizationStatus.Success or ElementAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAtRootLevelAsync(IUser currentUser, ISet<string> permissionsToCheck)
    {
        ElementAuthorizationStatus result =
            await _elementPermissionService.AuthorizeRootAccessAsync(currentUser, permissionsToCheck);

        // If we can't find the element item(s) then we can't determine whether you are denied access.
        return result is not (ElementAuthorizationStatus.Success or ElementAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAtRecycleBinLevelAsync(IUser currentUser, ISet<string> permissionsToCheck)
    {
        ElementAuthorizationStatus result =
            await _elementPermissionService.AuthorizeBinAccessAsync(currentUser, permissionsToCheck);

        // If we can't find the element item(s) then we can't determine whether you are denied access.
        return result is not (ElementAuthorizationStatus.Success or ElementAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedForCultures(IUser currentUser, ISet<string> culturesToCheck)
    {
        ElementAuthorizationStatus result =
            await _elementPermissionService.AuthorizeCultureAccessAsync(currentUser, culturesToCheck);

        // If we can't find the element item(s) then we can't determine whether you are denied access.
        return result is not (ElementAuthorizationStatus.Success or ElementAuthorizationStatus.NotFound);
    }
}