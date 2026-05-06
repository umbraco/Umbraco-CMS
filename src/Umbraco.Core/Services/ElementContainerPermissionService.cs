using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class ElementContainerPermissionService : IElementContainerPermissionService
{
    private readonly IEntityService _entityService;
    private readonly IUserService _userService;
    private readonly AppCaches _appCaches;

    public ElementContainerPermissionService(
        IEntityService entityService,
        IUserService userService,
        AppCaches appCaches)
    {
        _entityService = entityService;
        _userService = userService;
        _appCaches = appCaches;
    }

    /// <inheritdoc/>
    public Task<ElementAuthorizationStatus> AuthorizeAccessAsync(
        IUser user,
        IEnumerable<Guid> containerKeys,
        ISet<string> permissionsToCheck)
    {
        Guid[] keys = containerKeys.ToArray();
        if (keys.Length == 0)
        {
            return Task.FromResult(ElementAuthorizationStatus.NotFound);
        }

        // Use GetAllPaths instead of loading full content items - we only need paths for authorization
        TreeEntityPath[] entityPaths = _entityService.GetAllPaths([UmbracoObjectTypes.ElementContainer], keys).ToArray();
        if (entityPaths.Length == 0)
        {
            return Task.FromResult(ElementAuthorizationStatus.NotFound);
        }

        // Check path access using the paths directly
        int[]? startNodeIds = user.CalculateElementStartNodeIds(_entityService, _appCaches);
        foreach (TreeEntityPath entityPath in entityPaths)
        {
            if (ContentPermissions.HasPathAccess(entityPath.Path, startNodeIds, Constants.System.RecycleBinElement) == false)
            {
                return Task.FromResult(ElementAuthorizationStatus.UnauthorizedMissingPathAccess);
            }
        }

        return Task.FromResult(HasPermissionAccess(user, entityPaths.Select(p => p.Path), permissionsToCheck)
                ? ElementAuthorizationStatus.Success
                : ElementAuthorizationStatus.UnauthorizedMissingPermissionAccess);
    }

    /// <inheritdoc/>
    public Task<ElementAuthorizationStatus> AuthorizeRootAccessAsync(IUser user, ISet<string> permissionsToCheck)
    {
        var hasAccess = user.HasElementRootAccess(_entityService, _appCaches);

        if (hasAccess == false)
        {
            return Task.FromResult(ElementAuthorizationStatus.UnauthorizedMissingRootAccess);
        }

        // In this case, we have to use the Root id as path (i.e. -1) since we don't have a container item
        return Task.FromResult(HasPermissionAccess(user, new[] { Constants.System.RootString }, permissionsToCheck)
            ? ElementAuthorizationStatus.Success
            : ElementAuthorizationStatus.UnauthorizedMissingPermissionAccess);
    }

    /// <inheritdoc/>
    public Task<ElementAuthorizationStatus> AuthorizeBinAccessAsync(IUser user, ISet<string> permissionsToCheck)
    {
        var hasAccess = user.HasElementBinAccess(_entityService, _appCaches);

        if (hasAccess == false)
        {
            return Task.FromResult(ElementAuthorizationStatus.UnauthorizedMissingBinAccess);
        }

        // In this case, we have to use the Recycle Bin id as path (i.e. -22) since we don't have a container item
        return Task.FromResult(HasPermissionAccess(user, new[] { Constants.System.RecycleBinElementString }, permissionsToCheck)
            ? ElementAuthorizationStatus.Success
            : ElementAuthorizationStatus.UnauthorizedMissingPermissionAccess);
    }

    /// <summary>
    ///     Check the implicit/inherited permissions of a user for given element container items.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to check for access.</param>
    /// <param name="containerPaths">The paths of the element container items to check for access.</param>
    /// <param name="permissionsToCheck">The permissions to authorize.</param>
    /// <returns><c>true</c> if the user has the required permissions; otherwise, <c>false</c>.</returns>
    private bool HasPermissionAccess(IUser user, IEnumerable<string> containerPaths, ISet<string> permissionsToCheck)
    {
        foreach (var path in containerPaths)
        {
            // get the implicit/inherited permissions for the user for this path
            EntityPermissionSet permissionSet = _userService.GetPermissionsForPath(user, path);

            foreach (var p in permissionsToCheck)
            {
                if (permissionSet.GetAllPermissions().Contains(p) == false)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
