using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Umbraco.Web.Media.ThumbnailProviders;
using umbraco.BusinessLogic;
using Umbraco.Core.IO;
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
            var currentUser = GetCurrentUser();

            var parentMedia = new global::umbraco.cms.businesslogic.media.Media(parentId);
            AuthorizeAccess(parentMedia, currentUser);

            var data = new List<object>();

            // Get children and filter
            //TODO: Only fetch files, not containers
            //TODO: Cache responses to speed up susequent searches
            foreach (var child in parentMedia.Children)
            {
                var fileProp = child.getProperty("umbracoFile") ?? 
                    child.GenericProperties.FirstOrDefault(x => x.PropertyType.DataTypeDefinition.DataType.Id == new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"));

                var fileUrl = fileProp != null ? fileProp.Value.ToString() : "";
                var thumbUrl = ThumbnailProvidersResolver.Current.GetThumbnailUrl(fileUrl);
                var item = new
                {
                    Id = child.Id,
                    Path = child.Path,
                    Name = child.Text,
                    Tags = string.Join(",", Tag.GetTags(child.Id).Select(x => x.TagCaption)),
                    MediaTypeAlias = child.ContentType.Alias,
                    EditUrl = string.Format("editMedia.aspx?id={0}", child.Id),
                    FileUrl = fileUrl,
                    ThumbnailUrl = string.IsNullOrEmpty(thumbUrl)
                        ? IOHelper.ResolveUrl(SystemDirectories.Umbraco + "/images/thumbnails/" + child.ContentType.Thumbnail)
                        : thumbUrl
                };

                data.Add(item);
            }

            return new JavaScriptSerializer().Serialize(data);
        }

        [RestExtensionMethod(ReturnXml = false)]
        public static string Delete(string nodeIds)
        {
            var currentUser = GetCurrentUser();

            var nodeIdParts = nodeIds.Split(',');
            
            foreach (var nodeIdPart in nodeIdParts.Where(x => string.IsNullOrEmpty(x) == false))
            {
                int nodeId;
                if (Int32.TryParse(nodeIdPart, out nodeId) == false) 
                    continue;
                
                var node = new global::umbraco.cms.businesslogic.media.Media(nodeId);
                AuthorizeAccess(node, currentUser);

                node.delete(("," + node.Path + ",").Contains(",-21,"));
            }

            return new JavaScriptSerializer().Serialize(new
            {
                success = true
            });
        }

        private static User GetCurrentUser()
        {
            var currentUser = User.GetCurrent();
            if (currentUser == null)
                throw new UnauthorizedAccessException("You must be logged in to use this service");

            return currentUser;
        }

        private static void AuthorizeAccess(global::umbraco.cms.businesslogic.media.Media mediaItem, User currentUser)
        {
            if (("," + mediaItem.Path + ",").Contains("," + currentUser.StartMediaId + ",") == false)
                throw new UnauthorizedAccessException("You do not have access to this Media node");
        }
    }
}
