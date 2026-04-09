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

    /// <summary>
    ///     Represents the result of a content access check.
    /// </summary>
    public enum ContentAccess
    {
        /// <summary>
        ///     Access to the content is granted.
        /// </summary>
        Granted,

        /// <summary>
        ///     Access to the content is denied.
        /// </summary>
        Denied,

        /// <summary>
        ///     The content was not found.
        /// </summary>
        NotFound,
    }

    private readonly IContentService _contentService;
    private readonly IEntityService _entityService;
    private readonly IUserService _userService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissions" /> class.
    /// </summary>
    /// <param name="userService">The user service.</param>
    /// <param name="contentService">The content service.</param>
    /// <param name="entityService">The entity service.</param>
    /// <param name="appCaches">The application caches.</param>
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

    /// <summary>
    ///     Checks if the user has path access to a content item.
    /// </summary>
    /// <param name="path">The path of the content item.</param>
    /// <param name="startNodeIds">The user's start node IDs.</param>
    /// <param name="recycleBinId">The recycle bin ID.</param>
    /// <returns><c>true</c> if the user has access; otherwise, <c>false</c>.</returns>
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
    
    /// <summary>
    ///     Determines if a path is within the branch of the user's start node.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <param name="startNodeIds">The user's start node IDs.</param>
    /// <param name="startNodePaths">The user's start node paths.</param>
    /// <param name="hasPathAccess">Outputs whether the user has direct path access.</param>
    /// <returns><c>true</c> if the path is in the branch of the start node; otherwise, <c>false</c>.</returns>
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
