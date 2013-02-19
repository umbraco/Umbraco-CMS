using System.Collections.Generic;
using Umbraco.Core.Models;
using umbraco.BusinessLogic;

namespace umbraco.cms.businesslogic.media
{
    public class UmbracoImageMediaFactory : UmbracoMediaFactory
    {
        public override string MediaTypeAlias
        {
            get { return "Image"; }
        }

        public override List<string> Extensions
        {
            get { return new List<string> { "jpeg", "jpg", "gif", "bmp", "png", "tiff", "tif" }; }
        }

        public override void DoHandleMedia(Media media, PostedMediaFile postedFile, User user)
        {
            // Set media property to upload the file as well as set all related properties
            media.MediaItem.SetValue("umbracoFile", postedFile.FileName, postedFile.InputStream);

            // Copy back the values from the internal IMedia to ensure that the values are persisted when saved
            foreach (var property in media.MediaItem.Properties)
            {
                media.getProperty(property.Alias).Value = property.Value;
            }

            // Save media (using legacy media object to ensure the usage of the legacy events).
            media.Save();
        }
    }
}
