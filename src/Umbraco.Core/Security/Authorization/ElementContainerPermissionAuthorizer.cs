using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <inheritdoc />
internal sealed class ElementContainerPermissionAuthorizer : IElementContainerPermissionAuthorizer
{
    private readonly IElementContainerPermissionService _elementContainerPermissionService;

    public ElementContainerPermissionAuthorizer(IElementContainerPermissionService elementContainerPermissionService) =>
        _elementContainerPermissionService = elementContainerPermissionService;

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAsync(
        IUser currentUser,
        IEnumerable<Guid> containerKeys,
        ISet<string> permissionsToCheck)
    {
        Guid[] containerKeysAsArray = containerKeys as Guid[] ?? containerKeys.ToArray();
        if (containerKeysAsArray.Length == 0)
        {
            // Must succeed this requirement since we cannot process it.
            return true;
        }

        ElementAuthorizationStatus result =
            await _elementContainerPermissionService.AuthorizeAccessAsync(currentUser, containerKeysAsArray, permissionsToCheck);

        // If we can't find the container item(s) then we can't determine whether you are denied access.
        return result is not (ElementAuthorizationStatus.Success or ElementAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAtRootLevelAsync(IUser currentUser, ISet<string> permissionsToCheck)
    {
        ElementAuthorizationStatus result =
            await _elementContainerPermissionService.AuthorizeRootAccessAsync(currentUser, permissionsToCheck);

        // If we can't find the container item(s) then we can't determine whether you are denied access.
        return result is not (ElementAuthorizationStatus.Success or ElementAuthorizationStatus.NotFound);
    }

    /// <inheritdoc/>
    public async Task<bool> IsDeniedAtRecycleBinLevelAsync(IUser currentUser, ISet<string> permissionsToCheck)
    {
        ElementAuthorizationStatus result =
            await _elementContainerPermissionService.AuthorizeBinAccessAsync(currentUser, permissionsToCheck);

        // If we can't find the container item(s) then we can't determine whether you are denied access.
        return result is not (ElementAuthorizationStatus.Success or ElementAuthorizationStatus.NotFound);
    }
}
