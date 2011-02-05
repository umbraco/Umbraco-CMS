using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using umbraco.BusinessLogic;

namespace umbraco.cms.businesslogic.media
{
    public abstract class UmbracoMediaFactory : IMediaFactory
    {
        public abstract List<string> Extensions { get; }
        public virtual int Priority { get { return 1000; } }
        public abstract string MediaTypeAlias { get; }

        public virtual bool CanCreateMedia(int parentNodeId, PostedMediaFile postedFile, User user)
        {
            try
            {
                var parentNode = new Media(parentNodeId);

                return parentNodeId > -1 ?
                    user.Applications.Any(app => app.alias.ToLower() == "media") && (user.StartMediaId <= 0 || ("," + parentNode.Path + ",").Contains("," + user.StartMediaId + ",")) && parentNode.ContentType.AllowedChildContentTypeIDs.Contains(MediaType.GetByAlias(MediaTypeAlias).Id) :
                    true;
            }
            catch
            {
                return false;
            }
        }

        public Media CreateMedia(int parentNodeId, PostedMediaFile postedFile, User user)
        {
            var media = Media.MakeNew(postedFile.FileName,
                MediaType.GetByAlias(MediaTypeAlias),
                user,
                parentNodeId);

            if (postedFile.ContentLength > 0)
                HandleMedia(media, postedFile, user);

            media.XmlGenerate(new XmlDocument());

            return media;
        }

        public abstract void HandleMedia(Media media, PostedMediaFile uploadedFile, User user);

        #region Helper Methods

        public string ConstructDestPath(int propertyId)
        {
            if (UmbracoSettings.UploadAllowDirectories)
            {
                var path = VirtualPathUtility.Combine(VirtualPathUtility.AppendTrailingSlash(IO.SystemDirectories.Media), propertyId.ToString());

                return VirtualPathUtility.ToAbsolute(VirtualPathUtility.AppendTrailingSlash(path));
            }

            return VirtualPathUtility.ToAbsolute(VirtualPathUtility.AppendTrailingSlash(IO.SystemDirectories.Media));
        }

        public string ConstructDestFileName(int propertyId, string filename)
        {
            if (UmbracoSettings.UploadAllowDirectories)
                return filename;

            return propertyId + "-" + filename;
        }

        #endregion
    }
}
