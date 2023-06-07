using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Checks user access to media
/// </summary>
public class MediaPermissions
{
    private readonly AppCaches _appCaches;

    public enum MediaAccess
    {
        Granted,
        Denied,
        NotFound,
    }

    private readonly IEntityService _entityService;
    private readonly IMediaService _mediaService;

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
