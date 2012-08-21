using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using umbraco.BusinessLogic;
using Umbraco.Core.IO;

namespace umbraco.cms.businesslogic.media
{
    public class UmbracoFileMediaFactory : UmbracoMediaFactory
    {
        public override string MediaTypeAlias
        {
            get { return "File"; }
        }

        public override List<string> Extensions
        {
            get { return new List<string> { "*" }; }
        }

        public override void DoHandleMedia(Media media, PostedMediaFile uploadedFile, User user)
        {
            // Get umbracoFile property
            var propertyId = media.getProperty("umbracoFile").Id;

            // Get paths
            var destFilePath = _fileSystem.GetRelativePath(propertyId, uploadedFile.FileName);
            var ext = Path.GetExtension(destFilePath).Substring(1);

            //var absoluteDestPath = HttpContext.Current.Server.MapPath(destPath);
            //var absoluteDestFilePath = HttpContext.Current.Server.MapPath(destFilePath);

            // Set media properties
            media.getProperty("umbracoFile").Value = _fileSystem.GetUrl(destFilePath);
            media.getProperty("umbracoBytes").Value = uploadedFile.ContentLength;

            if (media.getProperty("umbracoExtension") != null)
                media.getProperty("umbracoExtension").Value = ext;

            if (media.getProperty("umbracoExtensio") != null)
                media.getProperty("umbracoExtensio").Value = ext;

            _fileSystem.AddFile(destFilePath, uploadedFile.InputStream, uploadedFile.ReplaceExisting);

            // Save media
            media.Save();
        }
    }
}
