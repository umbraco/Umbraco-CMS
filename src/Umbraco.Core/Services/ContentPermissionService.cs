using System.Globalization;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class ContentPermissionService : IContentPermissionService
{
    private readonly IContentService _contentService;
    private readonly IEntityService _entityService;
    private readonly IUserService _userService;
    private readonly AppCaches _appCaches;

    public ContentPermissionService(
        IContentService contentService,
        IEntityService entityService,
        IUserService userService,
        AppCaches appCaches)
    {
        _contentService = contentService;
        _entityService = entityService;
        _userService = userService;
        _appCaches = appCaches;
    }

    /// <inheritdoc/>
    public async Task<ContentAuthorizationStatus> AuthorizeAccessAsync(
        IUser performingUser,
        IEnumerable<Guid?> contentKeys,
        IReadOnlyList<char> permissionsToCheck)
    {
        var keysWithoutRoot = contentKeys
            .Where(x => x.HasValue)
            .Select(x => x!.Value);

        if (keysWithoutRoot.Count() < contentKeys.Count())
        {
            var authorizeRootAccess = await AuthorizeRootAccessAsync(performingUser);

            if (authorizeRootAccess != ContentAuthorizationStatus.Success)
            {
                return authorizeRootAccess;
            }
        }

        var contentItems = _contentService.GetByIds(keysWithoutRoot).ToArray();

        if (contentItems.Length == 0)
        {
            return ContentAuthorizationStatus.NotFound;
        }

        if (contentItems.Any(contentItem => performingUser.HasPathAccess(contentItem, _entityService, _appCaches) == false))
        {
            return ContentAuthorizationStatus.UnauthorizedMissingPathAccess;
        }

        return HasPermissionAccess(performingUser, contentItems.Select(c => c.Path), permissionsToCheck)
            ? ContentAuthorizationStatus.Success
            : ContentAuthorizationStatus.UnauthorizedMissingPathAccess;
    }

    /// <inheritdoc/>
    public async Task<ContentAuthorizationStatus> AuthorizeDescendantsAccessAsync(
        IUser performingUser,
        Guid parentKey,
        IReadOnlyList<char> permissionsToCheck)
    {
        var denied = new List<IUmbracoEntity>();
        var page = 0;
        const int pageSize = 500;
        var total = long.MaxValue;

        IContent? contentItem = _contentService.GetById(parentKey);

        if (contentItem is null)
        {
            return ContentAuthorizationStatus.NotFound;
        }

        while (page * pageSize < total)
        {
            // Order descendents by shallowest to deepest, this allows us to check permissions from top to bottom,
            // so we can exit early if a permission higher up fails.
            IEnumerable<IEntitySlim> descendants = _entityService.GetPagedDescendants(
                contentItem.Id,
                UmbracoObjectTypes.Document,
                page++,
                pageSize,
                out total,
                ordering: Ordering.By("path"));

            foreach (IEntitySlim descendant in descendants)
            {
                var hasPathAccess = performingUser.HasContentPathAccess(descendant, _entityService, _appCaches);
                var hasPermissionAccess = HasPermissionAccess(performingUser, new[] { descendant.Path }, permissionsToCheck);

                // If this item's path has already been denied or if the user doesn't have access to it, add to the deny list.
                if (denied.Any(x => descendant.Path.StartsWith($"{x.Path},")) || hasPathAccess == false || hasPermissionAccess == false)
                {
                    denied.Add(descendant);
                }
            }
        }

        return denied.Count == 0
            ? ContentAuthorizationStatus.Success
            : ContentAuthorizationStatus.UnauthorizedMissingDescendantAccess;
    }

    /// <inheritdoc/>
    public async Task<ContentAuthorizationStatus> AuthorizeRootAccessAsync(IUser performingUser, IReadOnlyList<char> permissionsToCheck)
    {
        var hasAccess = performingUser.HasContentRootAccess(_entityService, _appCaches);

        if (hasAccess == false)
        {
            return ContentAuthorizationStatus.UnauthorizedMissingRootAccess;
        }

        // In this case, we have to use the Root id as path (i.e. -1) since we don't have a content item
        return HasPermissionAccess(performingUser, new[] { Constants.System.RootString }, permissionsToCheck)
            ? ContentAuthorizationStatus.Success
            : ContentAuthorizationStatus.UnauthorizedMissingPathAccess;
    }

    /// <inheritdoc/>
    public async Task<ContentAuthorizationStatus> AuthorizeBinAccessAsync(IUser performingUser, IReadOnlyList<char> permissionsToCheck)
    {
        var hasAccess = performingUser.HasContentBinAccess(_entityService, _appCaches);

        if (hasAccess == false)
        {
            return ContentAuthorizationStatus.UnauthorizedMissingBinAccess;
        }

        // In this case, we have to use the Recycle Bin id as path (i.e. -20) since we don't have a content item
        return HasPermissionAccess(performingUser, new[] { Constants.System.RecycleBinContentString }, permissionsToCheck)
            ? ContentAuthorizationStatus.Success
            : ContentAuthorizationStatus.UnauthorizedMissingPathAccess;
    }

    /// <summary>
    ///     Check the implicit/inherited permissions for the user for the given content.
    /// </summary>
    /// <param name="user">The user performing the operation.</param>
    /// <param name="contentPaths">The paths of the content items to check for access.</param>
    /// <param name="permissionsToCheck">The permissions to authorize.</param>
    /// <returns><c>true</c> if the user has the required permissions; otherwise, <c>false</c>.</returns>
    private bool HasPermissionAccess(IUser user, IEnumerable<string> contentPaths, IEnumerable<char> permissionsToCheck)
    {
        foreach (var path in contentPaths)
        {
            // get the implicit/inherited permissions for the user for this path
            EntityPermissionSet permissionSet = _userService.GetPermissionsForPath(user, path);

            foreach (var p in permissionsToCheck)
            {
                if (permissionSet.GetAllPermissions().Contains(p.ToString(CultureInfo.InvariantCulture)) == false)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
