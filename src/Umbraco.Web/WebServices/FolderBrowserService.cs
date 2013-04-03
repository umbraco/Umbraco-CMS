using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Script.Serialization;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Web.Media.ThumbnailProviders;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.Tags;
using Umbraco.Web.BaseRest;

namespace Umbraco.Web.WebServices
{
	//TODO: Can we convert this to MVC please instead of /base?

    [RestExtension("FolderBrowserService")]
    public class FolderBrowserService
    {
        [RestExtensionMethod(ReturnXml = false)]
        public static string GetChildren(int parentId)
        {
            var service = ApplicationContext.Current.Services.EntityService;
            var parentMedia = service.Get(parentId, UmbracoObjectTypes.Media);
            var mediaPath = parentMedia == null ? parentId.ToString(CultureInfo.InvariantCulture) : parentMedia.Path;

            var currentUser = User.GetCurrent();
            var data = new List<object>();

            // Check user is logged in
            if (currentUser == null)
                throw new UnauthorizedAccessException("You must be logged in to use this service");

            // Check user is allowed to access selected media item
            if (!("," + mediaPath + ",").Contains("," + currentUser.StartMediaId + ","))
                throw new UnauthorizedAccessException("You do not have access to this Media node");

            // Get children and filter
            var entities = service.GetChildren(parentId, UmbracoObjectTypes.Media);
            //TODO: Only fetch files, not containers
            foreach (UmbracoEntity entity in entities)
            {
                var thumbUrl = ThumbnailProvidersResolver.Current.GetThumbnailUrl(entity.UmbracoFile);
                var item = new
                {
                    Id = entity.Id,
                    Path = entity.Path,
                    Name = entity.Name,
                    Tags = string.Join(",", Tag.GetTags(entity.Id).Select(x => x.TagCaption)),
                    MediaTypeAlias = entity.ContentTypeAlias,
                    EditUrl = string.Format("editMedia.aspx?id={0}", entity.Id),
                    FileUrl = entity.UmbracoFile,
                    ThumbnailUrl = !string.IsNullOrEmpty(thumbUrl)
                        ? thumbUrl
                        : IOHelper.ResolveUrl(SystemDirectories.Umbraco + "/images/thumbnails/" + entity.ContentTypeThumbnail)
                };

                data.Add(item);
            }

            return new JavaScriptSerializer().Serialize(data);
        }

        [RestExtensionMethod(ReturnXml = false)]
        public static string Delete(string nodeIds)
        {
            var nodeIdParts = nodeIds.Split(',');

            foreach (var nodeIdPart in nodeIdParts.Where(x => !string.IsNullOrEmpty(x)))
            {
                var nodeId = 0;
                if (!Int32.TryParse(nodeIdPart, out nodeId)) 
                    continue;
                
                var node = new global::umbraco.cms.businesslogic.media.Media(nodeId);
                node.delete(("," + node.Path + ",").Contains(",-21,"));
            }

            return new JavaScriptSerializer().Serialize(new
            {
                success = true
            });
        }
    }
}
