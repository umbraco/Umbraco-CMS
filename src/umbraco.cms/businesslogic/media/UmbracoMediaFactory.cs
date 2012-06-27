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

        public virtual bool CanHandleMedia(int parentNodeId, PostedMediaFile postedFile, User user)
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

        public Media HandleMedia(int parentNodeId, PostedMediaFile postedFile, User user)
        {
            return HandleMedia(parentNodeId, postedFile, user, false);
        }

        public Media HandleMedia(int parentNodeId, PostedMediaFile postedFile, User user, bool replaceExisting)
        {
            // Check to see if a file exists
            Media media;
            string mediaName = extractTitleFromFileName(postedFile.FileName);

            if (replaceExisting && TryFindExistingMedia(parentNodeId, postedFile.FileName, out media))
            {
                // Do nothing as existing media is returned
            }
            else
            {
                media = Media.MakeNew(mediaName,
                    MediaType.GetByAlias(MediaTypeAlias),
                    user,
                    parentNodeId);
            }

            if (postedFile.ContentLength > 0)
                DoHandleMedia(media, postedFile, user);

            media.XmlGenerate(new XmlDocument());

            return media;
        }

        public abstract void DoHandleMedia(Media media, PostedMediaFile uploadedFile, User user);

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

        public bool TryFindExistingMedia(int parentNodeId, string fileName, out Media existingMedia)
        {
            var children = parentNodeId == -1 ? Media.GetRootMedias() : new Media(parentNodeId).Children;
            foreach (var childMedia in children)
            {
                if (childMedia.ContentType.Alias == MediaTypeAlias)
                {
                    var prop = childMedia.getProperty("umbracoFile");
                    if (prop != null)
                    {
                        var destFileName = ConstructDestFileName(prop.Id, fileName);
                        var destPath = ConstructDestPath(prop.Id);
                        var destFilePath = VirtualPathUtility.Combine(destPath, destFileName);

                        if (prop.Value.ToString() == destFilePath)
                        {
                            existingMedia = childMedia;
                            return true;
                        }
                    }
                }
            }

            existingMedia = null;
            return false;
        }

        private string extractTitleFromFileName(string fileName)
        {
            // change the name
            string currentChar = String.Empty;
            string curName = fileName.Substring(0, fileName.LastIndexOf("."));
            int curNameLength = curName.Length;
            string friendlyName = String.Empty;
            for (int i = 0; i < curNameLength; i++)
            {
                currentChar = curName.Substring(i, 1);
                if (friendlyName.Length == 0)
                    currentChar = currentChar.ToUpper();

                if (i < curNameLength - 1 && friendlyName != "" && curName.Substring(i - 1, 1) == " ")
                    currentChar = currentChar.ToUpper();
                else if (currentChar != " " && i < curNameLength - 1 && friendlyName != ""
                && curName.Substring(i - 1, 1).ToUpper() != curName.Substring(i - 1, 1)
                && currentChar.ToUpper() == currentChar)
                    friendlyName += " ";

                friendlyName += currentChar;

            }

            return friendlyName;
        }

        #endregion
    }
}
