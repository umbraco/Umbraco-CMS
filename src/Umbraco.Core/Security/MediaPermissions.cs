using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Checks user access to media
/// </summary>
[Obsolete($"Please use {nameof(IMediaPermissionService)} instead, scheduled for removal in V15.")]
public class MediaPermissions
{
    private readonly AppCaches _appCaches;

    /// <summary>
    ///     Represents the result of a media access check.
    /// </summary>
    public enum MediaAccess
    {
        /// <summary>
        ///     Access to the media is granted.
        /// </summary>
        Granted,

        /// <summary>
        ///     Access to the media is denied.
        /// </summary>
        Denied,

        /// <summary>
        ///     The media was not found.
        /// </summary>
        NotFound,
    }

    private readonly IEntityService _entityService;
    private readonly IMediaService _mediaService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaPermissions" /> class.
    /// </summary>
    /// <param name="mediaService">The media service.</param>
    /// <param name="entityService">The entity service.</param>
    /// <param name="appCaches">The application caches.</param>
    public MediaPermissions(IMediaService mediaService, IEntityService entityService, AppCaches appCaches)
    {
        _mediaService = mediaService;
        _entityService = entityService;
        _appCaches = appCaches;
    }

    /// <summary>
    ///     Performs a permissions check for the user to check if it has access to the node based on
    ///     start node and/or permissions for the node
    /// </summary>
    /// <param name="user"></param>
    /// <param name="nodeId">The content to lookup, if the contentItem is not specified</param>
    /// <param name="media"></param>
    /// <returns></returns>
    public MediaAccess CheckPermissions(IUser? user, int nodeId, out IMedia? media)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        media = null;

        if (nodeId != Constants.System.Root && nodeId != Constants.System.RecycleBinMedia)
        {
            media = _mediaService.GetById(nodeId);
        }

        if (media == null && nodeId != Constants.System.Root && nodeId != Constants.System.RecycleBinMedia)
        {
            return MediaAccess.NotFound;
        }

        var hasPathAccess = nodeId == Constants.System.Root
            ? user.HasMediaRootAccess(_entityService, _appCaches)
            : nodeId == Constants.System.RecycleBinMedia
                ? user.HasMediaBinAccess(_entityService, _appCaches)
                : user.HasPathAccess(media, _entityService, _appCaches);

        return hasPathAccess ? MediaAccess.Granted : MediaAccess.Denied;
    }

    /// <summary>
    ///     Performs a permissions check for the user to check if it has access to the media item.
    /// </summary>
    /// <param name="media">The media item to check.</param>
    /// <param name="user">The user to check permissions for.</param>
    /// <returns>The result of the media access check.</returns>
    public MediaAccess CheckPermissions(IMedia? media, IUser? user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (media == null)
        {
            return MediaAccess.NotFound;
        }

        var hasPathAccess = user.HasPathAccess(media, _entityService, _appCaches);

        return hasPathAccess ? MediaAccess.Granted : MediaAccess.Denied;
    }
}
