using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.media
{
    [Obsolete("This is no longer used and will be removed from the codebase in future versions")]
    public class UmbracoFileMediaFactory : UmbracoMediaFactory
    {
        public override string MediaTypeAlias
        {
            get { return Constants.Conventions.MediaTypes.File; }
        }

        public override List<string> Extensions
        {
            get { return new List<string> { "*" }; }
        }

        public override void DoHandleMedia(Media media, PostedMediaFile uploadedFile, User user)
        {
            // Get umbracoFile property
            var propertyId = media.getProperty(Constants.Conventions.Media.File).Id;

            // Get paths
            var destFilePath = FileSystem.GetRelativePath(propertyId, uploadedFile.FileName);
            var ext = Path.GetExtension(destFilePath).Substring(1);

            //var absoluteDestPath = HttpContext.Current.Server.MapPath(destPath);
            //var absoluteDestFilePath = HttpContext.Current.Server.MapPath(destFilePath);

            // Set media properties
            media.getProperty(Constants.Conventions.Media.File).Value = FileSystem.GetUrl(destFilePath);
            media.getProperty(Constants.Conventions.Media.Bytes).Value = uploadedFile.ContentLength;

            if (media.getProperty(Constants.Conventions.Media.Extension) != null)
                media.getProperty(Constants.Conventions.Media.Extension).Value = ext;

            // Legacy: The 'extensio' typo applied to MySQL (bug in install script, prior to v4.6.x)
            if (media.getProperty("umbracoExtensio") != null)
                media.getProperty("umbracoExtensio").Value = ext;

            FileSystem.AddFile(destFilePath, uploadedFile.InputStream, uploadedFile.ReplaceExisting);

            // Save media
            media.Save();
        }
    }
}
