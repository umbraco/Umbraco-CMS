using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class ElementFolderPermissionService : IElementFolderPermissionService
{
    private readonly IEntityService _entityService;
    private readonly IUserService _userService;
    private readonly AppCaches _appCaches;

    public ElementFolderPermissionService(
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
        IEnumerable<Guid> folderKeys,
        ISet<string> permissionsToCheck)
    {
        Guid[] keys = folderKeys.ToArray();
        if (keys.Length == 0)
        {
            return Task.FromResult(ElementAuthorizationStatus.NotFound);
        }

        IEntitySlim[] entities = _entityService.GetAll(
            new[] { UmbracoObjectTypes.ElementContainer },
            keys).ToArray();

        if (entities.Length == 0)
        {
            return Task.FromResult(ElementAuthorizationStatus.NotFound);
        }

        if (entities.Any(entity => user.HasElementPathAccess(entity, _entityService, _appCaches) == false))
        {
            return Task.FromResult(ElementAuthorizationStatus.UnauthorizedMissingPathAccess);
        }

        return Task.FromResult(
            HasPermissionAccess(user, entities.Select(e => e.Path), permissionsToCheck)
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

        // In this case, we have to use the Root id as path (i.e. -1) since we don't have a folder item
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

        // In this case, we have to use the Recycle Bin id as path (i.e. -22) since we don't have a folder item
        return Task.FromResult(HasPermissionAccess(user, new[] { Constants.System.RecycleBinElementString }, permissionsToCheck)
            ? ElementAuthorizationStatus.Success
            : ElementAuthorizationStatus.UnauthorizedMissingPermissionAccess);
    }

    /// <summary>
    ///     Check the implicit/inherited permissions of a user for given element folder items.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to check for access.</param>
    /// <param name="folderPaths">The paths of the element folder items to check for access.</param>
    /// <param name="permissionsToCheck">The permissions to authorize.</param>
    /// <returns><c>true</c> if the user has the required permissions; otherwise, <c>false</c>.</returns>
    private bool HasPermissionAccess(IUser user, IEnumerable<string> folderPaths, ISet<string> permissionsToCheck)
    {
        foreach (var path in folderPaths)
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
