using System.Globalization;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class ContentPermissionService : IContentPermissionService
{
    private readonly IContentService _contentService;
    private readonly IEntityService _entityService;
    private readonly IUserService _userService;
    private readonly AppCaches _appCaches;
    private readonly ILanguageService _languageService;

    public ContentPermissionService(
        IContentService contentService,
        IEntityService entityService,
        IUserService userService,
        AppCaches appCaches,
        ILanguageService languageService)
    {
        _contentService = contentService;
        _entityService = entityService;
        _userService = userService;
        _appCaches = appCaches;
        _languageService = languageService;
    }

    /// <inheritdoc/>
    public Task<ContentAuthorizationStatus> AuthorizeAccessAsync(
        IUser user,
        IEnumerable<Guid> contentKeys,
        ISet<string> permissionsToCheck)
    {
        var contentItems = _contentService.GetByIds(contentKeys).ToArray();

        if (contentItems.Length == 0)
        {
            return Task.FromResult(ContentAuthorizationStatus.NotFound);
        }

        if (contentItems.Any(contentItem => user.HasPathAccess(contentItem, _entityService, _appCaches) == false))
        {
            return Task.FromResult(ContentAuthorizationStatus.UnauthorizedMissingPathAccess);
        }

        return Task.FromResult(HasPermissionAccess(user, contentItems.Select(c => c.Path), permissionsToCheck)
            ? ContentAuthorizationStatus.Success
            : ContentAuthorizationStatus.UnauthorizedMissingPermissionAccess);
    }

    /// <inheritdoc/>
    public Task<ContentAuthorizationStatus> AuthorizeDescendantsAccessAsync(
        IUser user,
        Guid parentKey,
        ISet<string> permissionsToCheck)
    {
        var denied = new List<IUmbracoEntity>();
        var page = 0;
        const int pageSize = 500;
        var total = long.MaxValue;

        IContent? contentItem = _contentService.GetById(parentKey);

        if (contentItem is null)
        {
            return Task.FromResult(ContentAuthorizationStatus.NotFound);
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
                var hasPathAccess = user.HasContentPathAccess(descendant, _entityService, _appCaches);
                var hasPermissionAccess = HasPermissionAccess(user, new[] { descendant.Path }, permissionsToCheck);

                // If this item's path has already been denied or if the user doesn't have access to it, add to the deny list.
                if (denied.Any(x => descendant.Path.StartsWith($"{x.Path},")) || hasPathAccess == false || hasPermissionAccess == false)
                {
                    denied.Add(descendant);
                }
            }
        }

        return Task.FromResult(denied.Count == 0
            ? ContentAuthorizationStatus.Success
            : ContentAuthorizationStatus.UnauthorizedMissingDescendantAccess);
    }

    /// <inheritdoc/>
    public Task<ContentAuthorizationStatus> AuthorizeRootAccessAsync(IUser user, ISet<string> permissionsToCheck)
    {
        var hasAccess = user.HasContentRootAccess(_entityService, _appCaches);

        if (hasAccess == false)
        {
            return Task.FromResult(ContentAuthorizationStatus.UnauthorizedMissingRootAccess);
        }

        // In this case, we have to use the Root id as path (i.e. -1) since we don't have a content item
        return Task.FromResult(HasPermissionAccess(user, new[] { Constants.System.RootString }, permissionsToCheck)
            ? ContentAuthorizationStatus.Success
            : ContentAuthorizationStatus.UnauthorizedMissingPermissionAccess);
    }

    /// <inheritdoc/>
    public Task<ContentAuthorizationStatus> AuthorizeBinAccessAsync(IUser user, ISet<string> permissionsToCheck)
    {
        var hasAccess = user.HasContentBinAccess(_entityService, _appCaches);

        if (hasAccess == false)
        {
            return Task.FromResult(ContentAuthorizationStatus.UnauthorizedMissingBinAccess);
        }

        // In this case, we have to use the Recycle Bin id as path (i.e. -20) since we don't have a content item
        return Task.FromResult(HasPermissionAccess(user, new[] { Constants.System.RecycleBinContentString }, permissionsToCheck)
            ? ContentAuthorizationStatus.Success
            : ContentAuthorizationStatus.UnauthorizedMissingPermissionAccess);
    }

    /// <inheritdoc/>
    public async Task<ContentAuthorizationStatus> AuthorizeCultureAccessAsync(IUser user, ISet<string> culturesToCheck)
    {
        if (user.Groups.Any(group => group.HasAccessToAllLanguages))
        {
            return ContentAuthorizationStatus.Success;
        }

        var allowedLanguages = user.Groups.SelectMany(g => g.AllowedLanguages).Distinct().ToArray();
        var allowedLanguageIsoCodes = await _languageService.GetIsoCodesByIdsAsync(allowedLanguages);

        return culturesToCheck.All(culture => allowedLanguageIsoCodes.InvariantContains(culture))
            ? ContentAuthorizationStatus.Success
            : ContentAuthorizationStatus.UnauthorizedMissingCulture;
    }

    /// <summary>
    ///     Check the implicit/inherited permissions of a user for given content items.
    /// </summary>
    /// <param name="user"><see cref="IUser" /> to check for access.</param>
    /// <param name="contentPaths">The paths of the content items to check for access.</param>
    /// <param name="permissionsToCheck">The permissions to authorize.</param>
    /// <returns><c>true</c> if the user has the required permissions; otherwise, <c>false</c>.</returns>
    private bool HasPermissionAccess(IUser user, IEnumerable<string> contentPaths, ISet<string> permissionsToCheck)
    {
        foreach (var path in contentPaths)
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
