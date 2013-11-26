using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using umbraco.BusinessLogic;
using Umbraco.Core;

namespace umbraco.cms.businesslogic.media
{
    [Obsolete("This is no longer used and will be removed from the codebase in future versions")]
    public abstract class UmbracoMediaFactory : IMediaFactory
    {
        public abstract List<string> Extensions { get; }
        public virtual int Priority { get { return 1000; } }
        public abstract string MediaTypeAlias { get; }

        internal readonly MediaFileSystem FileSystem;

        protected UmbracoMediaFactory()
        {
            FileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
        }

        public virtual bool CanHandleMedia(int parentNodeId, PostedMediaFile postedFile, User user)
        {
            try
            {
                var parentNode = new Media(parentNodeId);

                return parentNodeId <= -1 || user.Applications.Any(app => app.alias.ToLower() == Constants.Applications.Media) && (user.StartMediaId <= 0 || ("," + parentNode.Path + ",").Contains("," + user.StartMediaId + ",")) && parentNode.ContentType.AllowedChildContentTypeIDs.Contains(MediaType.GetByAlias(MediaTypeAlias).Id);
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
                    var prop = childMedia.getProperty(Constants.Conventions.Media.File);
                    if (prop != null && prop.Value != null)
                    {
                        int subfolderId;
                        var currentValue = prop.Value.ToString();

                        var subfolder = UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories
                            ? currentValue.Replace(FileSystem.GetUrl("/"), "").Split('/')[0]
                            : currentValue.Substring(currentValue.LastIndexOf("/", StringComparison.Ordinal) + 1).Split('-')[0];
                        
                        if (int.TryParse(subfolder, out subfolderId))
                        {
                            var destFilePath = FileSystem.GetRelativePath(subfolderId, fileName);
                            var destFileUrl = FileSystem.GetUrl(destFilePath);

                            if (prop.Value.ToString() == destFileUrl)
                            {
                                existingMedia = childMedia;
                                return true;
                            }
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
