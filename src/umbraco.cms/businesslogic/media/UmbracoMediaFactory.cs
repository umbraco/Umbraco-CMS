using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Umbraco.Core.IO;
using umbraco.BusinessLogic;

namespace umbraco.cms.businesslogic.media
{
    public abstract class UmbracoMediaFactory : IMediaFactory
    {
        public abstract List<string> Extensions { get; }
        public virtual int Priority { get { return 1000; } }
        public abstract string MediaTypeAlias { get; }

        internal readonly IMediaFileSystem FileSystem;

        protected UmbracoMediaFactory()
        {
            FileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<IMediaFileSystem>();
        }

        public virtual bool CanHandleMedia(int parentNodeId, PostedMediaFile postedFile, User user)
        {
            try
            {
                var parentNode = new Media(parentNodeId);

                return parentNodeId <= -1 || user.Applications.Any(app => app.alias.ToLower() == "media") && (user.StartMediaId <= 0 || ("," + parentNode.Path + ",").Contains("," + user.StartMediaId + ",")) && parentNode.ContentType.AllowedChildContentTypeIDs.Contains(MediaType.GetByAlias(MediaTypeAlias).Id);
            }
            catch
            {
                return false;
            }
        }

        public Media HandleMedia(int parentNodeId, PostedMediaFile postedFile, User user)
        {
            // Check to see if a file exists
            Media media;
            string mediaName = !string.IsNullOrEmpty(postedFile.DisplayName)
                ? postedFile.DisplayName
                : ExtractTitleFromFileName(postedFile.FileName);

            if (postedFile.ReplaceExisting && TryFindExistingMedia(parentNodeId, postedFile.FileName, out media))
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

        [Obsolete("Use HandleMedia(int, PostedMediaFile, User) and set the ReplaceExisting property on PostedMediaFile instead")]
        public Media HandleMedia(int parentNodeId, PostedMediaFile postedFile, User user, bool replaceExisting)
        {
            postedFile.ReplaceExisting = replaceExisting;

            return HandleMedia(parentNodeId, postedFile, user);
        }

        public abstract void DoHandleMedia(Media media, PostedMediaFile uploadedFile, User user);

        #region Helper Methods

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
                        var destFilePath = FileSystem.GetRelativePath(prop.Id, fileName);
                        var destFileUrl = FileSystem.GetUrl(destFilePath);

                        if (prop.Value.ToString() == destFileUrl)
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

        private string ExtractTitleFromFileName(string fileName)
        {
            // change the name
            var curName = fileName.Substring(0, fileName.LastIndexOf(".", StringComparison.Ordinal)).ToCharArray();
            var curNameLength = curName.Length;
            var friendlyName = String.Empty;

            for (var i = 0; i < curNameLength; i++)
            {
                var currentChar = curName[i];
                var currentString = String.Empty;
                
                if (Char.IsSeparator(currentChar) || Char.IsWhiteSpace(currentChar) || (Char.IsPunctuation(currentChar) 
                    && (currentChar == '_' || currentChar == '-' || currentChar == '.' || currentChar == '%')))
                {
                    currentString = " ";
                } 
                else if (Char.IsPunctuation(currentChar) || Char.IsLetterOrDigit(currentChar))
                {
                    currentString = currentChar.ToString(CultureInfo.InvariantCulture);
                }
                
                friendlyName += currentString;
            }
            
            //Capitalize each first letter of a word
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            var textInfo = cultureInfo.TextInfo;

            friendlyName = textInfo.ToTitleCase(friendlyName);

            //Remove multiple consecutive spaces
            var regex = new Regex(@"[ ]{2,}", RegexOptions.None);
            friendlyName = regex.Replace(friendlyName, @" ");

            return friendlyName;
        }

        #endregion
    }
}
