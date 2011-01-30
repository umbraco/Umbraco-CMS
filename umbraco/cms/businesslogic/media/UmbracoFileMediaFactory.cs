using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using umbraco.BusinessLogic;

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

        public override void HandleMedia(Media media, PostedMediaFile uploadedFile, User user)
        {
            // Get umbracoFile property
            var propertyId = media.getProperty("umbracoFile").Id;

            // Get paths
            var destFileName = ConstructDestFileName(propertyId, uploadedFile.FileName);
            var destPath = ConstructDestPath(propertyId);
            var destFilePath = VirtualPathUtility.Combine(destPath, destFileName);
            var ext = VirtualPathUtility.GetExtension(destFileName).Substring(1);

            var absoluteDestPath = HttpContext.Current.Server.MapPath(destPath);
            var absoluteDestFilePath = HttpContext.Current.Server.MapPath(destFilePath);

            // Set media properties
            media.getProperty("umbracoFile").Value = destFilePath;
            media.getProperty("umbracoBytes").Value = uploadedFile.ContentLength;
            media.getProperty("umbracoExtension").Value = ext;

            // Create directory
            if (UmbracoSettings.UploadAllowDirectories)
                Directory.CreateDirectory(absoluteDestPath);

            // Save file
            uploadedFile.SaveAs(absoluteDestFilePath);

            // Close stream
            uploadedFile.InputStream.Close();

            // Save media
            media.Save();
        }
    }
}
