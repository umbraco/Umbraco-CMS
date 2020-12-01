using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// Checks user access to media
    /// </summary>
    public class MediaPermissions
    {
        private readonly IMediaService _mediaService;
        private readonly IEntityService _entityService;

        public enum MediaAccess
        {
            Granted,
            Denied,
            NotFound
        }

        public MediaPermissions(IMediaService mediaService, IEntityService entityService)
        {
            _mediaService = mediaService;
            _entityService = entityService;
        }

        /// <summary>
        /// Performs a permissions check for the user to check if it has access to the node based on
        /// start node and/or permissions for the node
        /// </summary>
        /// <param name="user"></param>
        /// <param name="mediaService"></param>
        /// <param name="entityService"></param>
        /// <param name="nodeId">The content to lookup, if the contentItem is not specified</param>
        /// <returns></returns>
        public MediaAccess CheckPermissions(IUser user, int nodeId, out IMedia media)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            media = null;

            if (nodeId != Constants.System.Root && nodeId != Constants.System.RecycleBinMedia)
            {
                media = _mediaService.GetById(nodeId);
            }

            if (media == null && nodeId != Constants.System.Root && nodeId != Constants.System.RecycleBinMedia)
            {
                return MediaAccess.NotFound;
            }

            var hasPathAccess = (nodeId == Constants.System.Root)
                ? user.HasMediaRootAccess(_entityService)
                : (nodeId == Constants.System.RecycleBinMedia)
                    ? user.HasMediaBinAccess(_entityService)
                    : user.HasPathAccess(media, _entityService);

            return hasPathAccess ? MediaAccess.Granted : MediaAccess.Denied;
        }

        public MediaAccess CheckPermissions(IMedia media, IUser user)
        {            
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (media == null) return MediaAccess.NotFound;

            var hasPathAccess = user.HasPathAccess(media, _entityService);

            return hasPathAccess ? MediaAccess.Granted : MediaAccess.Denied;
        }
    }
}
