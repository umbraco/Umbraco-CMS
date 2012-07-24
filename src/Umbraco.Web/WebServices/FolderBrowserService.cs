using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Umbraco.Core;
using Umbraco.Web.Media.ThumbnailProviders;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.IO;
using umbraco.cms.businesslogic.Tags;
using umbraco.cms.businesslogic.media;
using umbraco.presentation.umbracobase;

namespace Umbraco.Web.WebServices
{
    [RestExtension("FolderBrowserService")]
    public class FolderBrowserService
    {
        //[RestExtensionMethod(returnXml = false)]
        //public static string GetChildNodes(int parentId)
        //{
        //    return GetChildNodes(parentId, "");
        //}

        [RestExtensionMethod(returnXml = false)]
        public static string GetChildNodes(int parentId, string filterTerm)
        {
            var parentMedia = new global::umbraco.cms.businesslogic.media.Media(parentId);
            var currentUser = User.GetCurrent();
            var data = new List<object>();

            // Check user is logged in
            if (currentUser == null)
                throw new UnauthorizedAccessException("You must be logged in to use this service");

            // Check user is allowed to access selected media item
            if(!("," + parentMedia.Path + ",").Contains("," + currentUser.StartMediaId + ","))
                throw new UnauthorizedAccessException("You do not have access to this Media node");

            // Get children and filter
            //TODO: Only fetch files, not containers
            //TODO: Cache responses to speed up susequent searches
            foreach (var child in parentMedia.Children.Where(x => string.IsNullOrEmpty(filterTerm) ||
                x.Text.InvariantContains(filterTerm) ||
                Tag.GetTags(x.Id).Any(y => y.TagCaption.InvariantContains(filterTerm))))
            {
                var fileProp = child.getProperty("umbracoFile") ?? 
                    child.GenericProperties.FirstOrDefault(x =>
                        x.PropertyType.DataTypeDefinition.DataType.Id == new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"));

                var fileUrl = fileProp != null ? fileProp.Value.ToString() : "";
                var thumbUrl = ThumbnailProviderManager.GetThumbnailUrl(fileUrl);
                
                data.Add(new
                {
                    Id = child.Id,
                    Name = child.Text,
                    MediaTypeAlias = child.ContentType.Alias,
                    FileUrl = fileUrl,
                    ThumbnailUrl = !string.IsNullOrEmpty(thumbUrl) 
                        ? thumbUrl 
                        : IOHelper.ResolveUrl(SystemDirectories.Umbraco + "/images/thumbnails/" + child.ContentType.Thumbnail)
                });
            }

            return new JavaScriptSerializer().Serialize(data);
        }
    }
}
