using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Content service extension methods
    /// </summary>
    public static class ContentServiceExtensions
    {
        public static IEnumerable<IContent> GetByIds(this IContentService contentService, IEnumerable<Udi> ids)
        {
            var guids = new List<GuidUdi>();
            foreach (var udi in ids)
            {
                var guidUdi = udi as GuidUdi;
                if (guidUdi == null)
                    throw new InvalidOperationException("The UDI provided isn't of type " + typeof(GuidUdi) + " which is required by content");
                guids.Add(guidUdi);
            }

            return contentService.GetByIds(guids.Select(x => x.Guid));
        }

        /// <summary>
        /// Method to create an IContent object based on the Udi of a parent
        /// </summary>
        /// <param name="contentService"></param>
        /// <param name="name"></param>
        /// <param name="parentId"></param>
        /// <param name="mediaTypeAlias"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static IContent CreateContent(this IContentService contentService, string name, Udi parentId, string mediaTypeAlias, int userId = 0)
        {
            var guidUdi = parentId as GuidUdi;
            if (guidUdi == null)
                throw new InvalidOperationException("The UDI provided isn't of type " + typeof(GuidUdi) + " which is required by content");
            var parent = contentService.GetById(guidUdi.Guid);
            return contentService.Create(name, parent, mediaTypeAlias, userId);
        }

        /// <summary>
        /// Remove all permissions for this user for all nodes
        /// </summary>
        /// <param name="contentService"></param>
        /// <param name="contentId"></param>
        public static void RemoveContentPermissions(this IContentService contentService, int contentId)
        {
            contentService.SetPermissions(new EntityPermissionSet(contentId, new EntityPermissionCollection()));
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
