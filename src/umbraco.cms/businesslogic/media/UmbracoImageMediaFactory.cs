using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using umbraco.BusinessLogic;
using Umbraco.Core;

namespace umbraco.cms.businesslogic.media
{
    [Obsolete("This interface is no longer used and will be removed from the codebase in future versions")]
    public class UmbracoImageMediaFactory : UmbracoMediaFactory
    {
        public override string MediaTypeAlias
        {
            get { return Constants.Conventions.MediaTypes.Image; }
        }

        public override List<string> Extensions
        {
            get { return new List<string> { "jpeg", "jpg", "gif", "bmp", "png", "tiff", "tif" }; }
        }

        public override void DoHandleMedia(Media media, PostedMediaFile postedFile, User user)
        {
            // Set media property to upload the file as well as set all related properties
            media.MediaItem.SetValue(Constants.Conventions.Media.File, postedFile.FileName, postedFile.InputStream);

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
