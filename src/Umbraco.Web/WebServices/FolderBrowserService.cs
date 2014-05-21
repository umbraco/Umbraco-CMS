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
using Tag = umbraco.cms.businesslogic.Tags.Tag;

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
            AuthorizeAccess(parentId, currentUser);

            // Get children and filter
            var data = new List<object>();
            var service = ApplicationContext.Current.Services.EntityService;

            var entities = service.GetChildren(parentId, UmbracoObjectTypes.Media);
            foreach (UmbracoEntity entity in entities)
            {
                var uploadFieldProperty = entity.AdditionalData
                    .Select(x => x.Value as UmbracoEntity.EntityProperty)
                    .Where(x => x != null)
                    .FirstOrDefault(x => x.PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias);
                    
                //var uploadFieldProperty = entity.UmbracoProperties.FirstOrDefault(x => x.PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias);
                
                var thumbnailUrl = uploadFieldProperty == null ? "" : ThumbnailProvidersResolver.Current.GetThumbnailUrl((string)uploadFieldProperty.Value);

                var item = new
                {
                    Id = entity.Id,
                    Path = entity.Path,
                    Name = entity.Name,
                    Tags = string.Join(",", Tag.GetTags(entity.Id).Select(x => x.TagCaption)),
                    MediaTypeAlias = entity.ContentTypeAlias,
                    EditUrl = string.Format("editMedia.aspx?id={0}", entity.Id),
                    FileUrl = uploadFieldProperty == null 
                        ? "" 
                        : uploadFieldProperty.Value,
                    ThumbnailUrl = string.IsNullOrEmpty(thumbnailUrl)
                        ? IOHelper.ResolveUrl(string.Format("{0}/images/thumbnails/{1}", SystemDirectories.Umbraco, entity.ContentTypeThumbnail))
                        : thumbnailUrl
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

            return new JavaScriptSerializer().Serialize(new { success = true });
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

        private static void AuthorizeAccess(int parentId, User currentUser)
        {
            var service = ApplicationContext.Current.Services.EntityService;
            var parentMedia = service.Get(parentId, UmbracoObjectTypes.Media);
            var mediaPath = parentMedia == null ? parentId.ToString(CultureInfo.InvariantCulture) : parentMedia.Path;

            if (("," + mediaPath + ",").Contains("," + currentUser.StartMediaId + ",") == false)
                throw new UnauthorizedAccessException("You do not have access to this Media node");
        }
    }
}
