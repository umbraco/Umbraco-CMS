using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class ElementPermissionService : IElementPermissionService
{
    private readonly IEntityService _entityService;
    private readonly IUserService _userService;
    private readonly AppCaches _appCaches;
    private readonly ILanguageService _languageService;

    public ElementPermissionService(
        IEntityService entityService,
        IUserService userService,
        AppCaches appCaches,
        ILanguageService languageService)
    {
        _entityService = entityService;
        _userService = userService;
        _appCaches = appCaches;
        _languageService = languageService;
    }

    /// <inheritdoc/>
    public Task<ElementAuthorizationStatus> AuthorizeAccessAsync(
        IUser user,
        IEnumerable<Guid> elementKeys,
        ISet<string> permissionsToCheck)
    {
        Guid[] keys = elementKeys.ToArray();
        if (keys.Length == 0)
        {
            return Task.FromResult(ElementAuthorizationStatus.NotFound);
        }

        // Fetch both Elements and ElementContainers (folders)
        IEntitySlim[] entities = _entityService.GetAll(
            new[] { UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer },
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
    public Task<ElementAuthorizationStatus> AuthorizeDescendantsAccessAsync(
        IUser user,
        Guid parentKey,
        ISet<string> permissionsToCheck)
    {
        var denied = new List<IUmbracoEntity>();
        var skip = 0;
        const int take = 500;
        var total = long.MaxValue;

        UmbracoObjectTypes[] objectTypes = { UmbracoObjectTypes.Element, UmbracoObjectTypes.ElementContainer };

        // Try to find the parent as either Element or ElementContainer
        IEntitySlim? parentEntity = _entityService.GetAll(objectTypes, parentKey).FirstOrDefault();

        if (parentEntity is null)
        {
            return Task.FromResult(ElementAuthorizationStatus.NotFound);
        }

        UmbracoObjectTypes parentObjectType = ObjectTypes.GetUmbracoObjectType(parentEntity.NodeObjectType);

        while (skip < total)
        {
            // Order descendants by shallowest to deepest, this allows us to check permissions from top to bottom,
            // so we can exit early if a permission higher up fails.
            IEnumerable<IEntitySlim> descendants = _entityService.GetPagedDescendants(
                parentKey,
                parentObjectType,
                objectTypes,
                skip,
                take,
                out total,
                ordering: Ordering.By("path"));

            skip += take;

            foreach (IEntitySlim descendant in descendants)
            {
                var hasPathAccess = user.HasElementPathAccess(descendant, _entityService, _appCaches);
                var hasPermissionAccess = HasPermissionAccess(user, new[] { descendant.Path }, permissionsToCheck);

                // If this item's path has already been denied or if the user doesn't have access to it, add to the deny list.
                if (denied.Any(x => descendant.Path.StartsWith($"{x.Path},")) || hasPathAccess == false || hasPermissionAccess == false)
                {
                    denied.Add(descendant);
                }
            }
        }

        return Task.FromResult(denied.Count == 0
            ? ElementAuthorizationStatus.Success
            : ElementAuthorizationStatus.UnauthorizedMissingDescendantAccess);
    }

    /// <inheritdoc/>
    public Task<ElementAuthorizationStatus> AuthorizeRootAccessAsync(IUser user, ISet<string> permissionsToCheck)
    {
        var hasAccess = user.HasElementRootAccess(_entityService, _appCaches);

        if (hasAccess == false)
        {
            return Task.FromResult(ElementAuthorizationStatus.UnauthorizedMissingRootAccess);
        }

        // In this case, we have to use the Root id as path (i.e. -1) since we don't have an element item
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

        // In this case, we have to use the Recycle Bin id as path (i.e. -22) since we don't have an element item
        return Task.FromResult(HasPermissionAccess(user, new[] { Constants.System.RecycleBinElementString }, permissionsToCheck)
            ? ElementAuthorizationStatus.Success
            : ElementAuthorizationStatus.UnauthorizedMissingPermissionAccess);
    }

    /// <inheritdoc/>
    public async Task<ElementAuthorizationStatus> AuthorizeCultureAccessAsync(IUser user, ISet<string> culturesToCheck)
    {
        if (user.Groups.Any(group => group.HasAccessToAllLanguages))
        {
            return ElementAuthorizationStatus.Success;
        }

        var allowedLanguages = user.Groups.SelectMany(g => g.AllowedLanguages).Distinct().ToArray();
        var allowedLanguageIsoCodes = await _languageService.GetIsoCodesByIdsAsync(allowedLanguages);

        return culturesToCheck.All(culture => allowedLanguageIsoCodes.InvariantContains(culture))
            ? ElementAuthorizationStatus.Success
            : ElementAuthorizationStatus.UnauthorizedMissingCulture;
    }

    /// <summary>
    ///     Check the implicit/inherited permissions of a user for given element items.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to check for access.</param>
    /// <param name="elementPaths">The paths of the element items to check for access.</param>
    /// <param name="permissionsToCheck">The permissions to authorize.</param>
    /// <returns><c>true</c> if the user has the required permissions; otherwise, <c>false</c>.</returns>
    private bool HasPermissionAccess(IUser user, IEnumerable<string> elementPaths, ISet<string> permissionsToCheck)
    {
        foreach (var path in elementPaths)
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
