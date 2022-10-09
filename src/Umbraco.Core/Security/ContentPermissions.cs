using System.Globalization;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Checks user access to content
/// </summary>
public class ContentPermissions
{
    private readonly AppCaches _appCaches;

    public enum ContentAccess
    {
        Granted,
        Denied,
        NotFound,
    }

    private readonly IContentService _contentService;
    private readonly IEntityService _entityService;
    private readonly IUserService _userService;

    public ContentPermissions(
        IUserService userService,
        IContentService contentService,
        IEntityService entityService,
        AppCaches appCaches)
    {
        _userService = userService;
        _contentService = contentService;
        _entityService = entityService;
        _appCaches = appCaches;
    }

    public static bool HasPathAccess(string? path, int[]? startNodeIds, int recycleBinId)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
        }

        // check for no access
        if (startNodeIds is null || startNodeIds.Length == 0)
        {
            return false;
        }

        // check for root access
        if (startNodeIds.Contains(Constants.System.Root))
        {
            return true;
        }

        var formattedPath = string.Concat(",", path, ",");

        // only users with root access have access to the recycle bin,
        // if the above check didn't pass then access is denied
        if (formattedPath.Contains(string.Concat(",", recycleBinId.ToString(CultureInfo.InvariantCulture), ",")))
        {
            return false;
        }

        // check for a start node in the path
        return startNodeIds.Any(x =>
            formattedPath.Contains(string.Concat(",", x.ToString(CultureInfo.InvariantCulture), ",")));
    }

    public ContentAccess CheckPermissions(
        IContent content,
        IUser user,
        char permissionToCheck) => CheckPermissions(content, user, new[] { permissionToCheck });

    public ContentAccess CheckPermissions(
        IContent? content,
        IUser? user,
        IReadOnlyList<char> permissionsToCheck)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (content == null)
        {
            return ContentAccess.NotFound;
        }

        var hasPathAccess = user.HasPathAccess(content, _entityService, _appCaches);

        if (hasPathAccess == false)
        {
            return ContentAccess.Denied;
        }

        if (permissionsToCheck == null || permissionsToCheck.Count == 0)
        {
            return ContentAccess.Granted;
        }

        // get the implicit/inherited permissions for the user for this path
        return CheckPermissionsPath(content.Path, user, permissionsToCheck)
            ? ContentAccess.Granted
            : ContentAccess.Denied;
    }

    public ContentAccess CheckPermissions(
        IUmbracoEntity entity,
        IUser? user,
        char permissionToCheck) => CheckPermissions(entity, user, new[] { permissionToCheck });

    public ContentAccess CheckPermissions(
        IUmbracoEntity entity,
        IUser? user,
        IReadOnlyList<char> permissionsToCheck)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (entity == null)
        {
            return ContentAccess.NotFound;
        }

        var hasPathAccess = user.HasContentPathAccess(entity, _entityService, _appCaches);

        if (hasPathAccess == false)
        {
            return ContentAccess.Denied;
        }

        if (permissionsToCheck == null || permissionsToCheck.Count == 0)
        {
            return ContentAccess.Granted;
        }

        // get the implicit/inherited permissions for the user for this path
        return CheckPermissionsPath(entity.Path, user, permissionsToCheck)
            ? ContentAccess.Granted
            : ContentAccess.Denied;
    }

    /// <summary>
    ///     Checks if the user has access to the specified node and permissions set
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="user"></param>
    /// <param name="userService"></param>
    /// <param name="entityService"></param>
    /// <param name="entity">The <see cref="IUmbracoEntity" /> item resolved if one was found for the id</param>
    /// <param name="permissionsToCheck"></param>
    /// <returns></returns>
    public ContentAccess CheckPermissions(
        int nodeId,
        IUser user,
        out IUmbracoEntity? entity,
        IReadOnlyList<char>? permissionsToCheck = null)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (permissionsToCheck == null)
        {
            permissionsToCheck = Array.Empty<char>();
        }

        bool? hasPathAccess = null;
        entity = null;

        if (nodeId == Constants.System.Root)
        {
            hasPathAccess = user.HasContentRootAccess(_entityService, _appCaches);
        }
        else if (nodeId == Constants.System.RecycleBinContent)
        {
            hasPathAccess = user.HasContentBinAccess(_entityService, _appCaches);
        }

        if (hasPathAccess.HasValue)
        {
            return hasPathAccess.Value ? ContentAccess.Granted : ContentAccess.Denied;
        }

        entity = _entityService.Get(nodeId, UmbracoObjectTypes.Document);
        if (entity == null)
        {
            return ContentAccess.NotFound;
        }

        hasPathAccess = user.HasContentPathAccess(entity, _entityService, _appCaches);

        if (hasPathAccess == false)
        {
            return ContentAccess.Denied;
        }

        if (permissionsToCheck == null || permissionsToCheck.Count == 0)
        {
            return ContentAccess.Granted;
        }

        // get the implicit/inherited permissions for the user for this path
        return CheckPermissionsPath(entity.Path, user, permissionsToCheck)
            ? ContentAccess.Granted
            : ContentAccess.Denied;
    }

    /// <summary>
    ///     Checks if the user has access to the specified node and permissions set
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="user"></param>
    /// <param name="userService"></param>
    /// <param name="contentService"></param>
    /// <param name="entityService"></param>
    /// <param name="contentItem">The <see cref="IContent" /> item resolved if one was found for the id</param>
    /// <param name="permissionsToCheck"></param>
    /// <returns></returns>
    public ContentAccess CheckPermissions(
        int nodeId,
        IUser? user,
        out IContent? contentItem,
        IReadOnlyList<char>? permissionsToCheck = null)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (permissionsToCheck == null)
        {
            permissionsToCheck = Array.Empty<char>();
        }

        bool? hasPathAccess = null;
        contentItem = null;

        if (nodeId == Constants.System.Root)
        {
            hasPathAccess = user.HasContentRootAccess(_entityService, _appCaches);
        }
        else if (nodeId == Constants.System.RecycleBinContent)
        {
            hasPathAccess = user.HasContentBinAccess(_entityService, _appCaches);
        }

        if (hasPathAccess.HasValue)
        {
            return hasPathAccess.Value ? ContentAccess.Granted : ContentAccess.Denied;
        }

        contentItem = _contentService.GetById(nodeId);
        if (contentItem == null)
        {
            return ContentAccess.NotFound;
        }

        hasPathAccess = user.HasPathAccess(contentItem, _entityService, _appCaches);

        if (hasPathAccess == false)
        {
            return ContentAccess.Denied;
        }

        if (permissionsToCheck == null || permissionsToCheck.Count == 0)
        {
            return ContentAccess.Granted;
        }

        // get the implicit/inherited permissions for the user for this path
        return CheckPermissionsPath(contentItem.Path, user, permissionsToCheck)
            ? ContentAccess.Granted
            : ContentAccess.Denied;
    }

    private bool CheckPermissionsPath(string? path, IUser user, IReadOnlyList<char>? permissionsToCheck = null)
    {
        if (permissionsToCheck == null)
        {
            permissionsToCheck = Array.Empty<char>();
        }

        // get the implicit/inherited permissions for the user for this path,
        // if there is no content item for this id, than just use the id as the path (i.e. -1 or -20)
        EntityPermissionSet permission = _userService.GetPermissionsForPath(user, path);

        var allowed = true;
        foreach (var p in permissionsToCheck)
        {
            if (permission == null
                || permission.GetAllPermissions().Contains(p.ToString(CultureInfo.InvariantCulture)) == false)
            {
                allowed = false;
            }
        }

        return allowed;
    }

    public static bool IsInBranchOfStartNode(string path, int[]? startNodeIds, string[]? startNodePaths, out bool hasPathAccess)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
        }

        hasPathAccess = false;

        // check for no access
        if (startNodeIds?.Length == 0)
        {
            return false;
        }

        // check for root access
        if (startNodeIds?.Contains(Constants.System.Root) ?? false)
        {
            hasPathAccess = true;
            return true;
        }

        // is it self?
        var self = startNodePaths?.Any(x => x == path) ?? false;
        if (self)
        {
            hasPathAccess = true;
            return true;
        }

        // is it ancestor?
        var ancestor = startNodePaths?.Any(x => x.StartsWith(path)) ?? false;
        if (ancestor)
        {
            // hasPathAccess = false;
            return true;
        }

        // is it descendant?
        var descendant = startNodePaths?.Any(x => path.StartsWith(x)) ?? false;
        if (descendant)
        {
            hasPathAccess = true;
            return true;
        }

        return false;
    }
}
