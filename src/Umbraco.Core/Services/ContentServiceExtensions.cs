using System.Linq;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Services
{
    public static class ContentServiceExtensions
    {
        /// <summary>
        /// Remove all permissions for this user for all nodes
        /// </summary>
        /// <param name="contentService"></param>
        /// <param name="contentId"></param>
        public static void RemoveContentPermissions(this IContentService contentService, int contentId)
        {
            contentService.ReplaceContentPermissions(new EntityPermissionSet(contentId, Enumerable.Empty<EntityPermissionSet.UserPermission>()));
        }

        /// <summary>
        /// Returns true if there is any content in the recycle bin
        /// </summary>
        /// <param name="contentService"></param>
        /// <returns></returns>
        public static bool RecycleBinSmells(this IContentService contentService)
        {
            return contentService.CountChildren(Constants.System.RecycleBinContent) > 0;
        }

        /// <summary>
        /// Returns true if there is any media in the recycle bin
        /// </summary>
        /// <param name="mediaService"></param>
        /// <returns></returns>
        public static bool RecycleBinSmells(this IMediaService mediaService)
        {
            return mediaService.CountChildren(Constants.System.RecycleBinMedia) > 0;
        }
    }
}