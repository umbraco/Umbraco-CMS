using System;
using System.IO;
using System.Linq;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Composing;

namespace Umbraco.Tests.LegacyXmlPublishedCache
{
    class PreviewContent
    {
        private readonly int _userId;
        private readonly Guid _previewSet;
        private string _previewSetPath;
        private XmlDocument _previewXml;
        private readonly XmlStore _xmlStore;

        /// <summary>
        /// Gets the XML document.
        /// </summary>
        /// <remarks>May return <c>null</c> if the preview content set is invalid.</remarks>
        public XmlDocument XmlContent
        {
            get
            {
                // null if invalid preview content
                if (_previewSetPath == null) return null;

                // load if not loaded yet
                if (_previewXml != null)
                    return _previewXml;

                _previewXml = new XmlDocument();

                try
                {
                    _previewXml.Load(_previewSetPath);
                }
                catch (Exception ex)
                {
                    Current.Logger.Error<PreviewContent,Guid,int>(ex, "Could not load preview set {PreviewSet} for user {UserId}.", _previewSet, _userId);

                    ClearPreviewSet();

                    _previewXml = null;
                    _previewSetPath = null; // do not try again
                }

                return _previewXml;
            }
        }

        /// <summary>
        /// Gets the preview token.
        /// </summary>
        /// <remarks>To be stored in a cookie or wherever appropriate.</remarks>
        public string Token => _userId + ":" + _previewSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="PreviewContent"/> class for a user.
        /// </summary>
        /// <param name="xmlStore">The underlying Xml store.</param>
        /// <param name="userId">The user identifier.</param>
        public PreviewContent(XmlStore xmlStore, int userId)
        {
            if (xmlStore == null)
                throw new ArgumentNullException(nameof(xmlStore));
            _xmlStore = xmlStore;

            _userId = userId;
            _previewSet = Guid.NewGuid();
            _previewSetPath = GetPreviewSetPath(_userId, _previewSet);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PreviewContent"/> with a preview token.
        /// </summary>
        /// <param name="xmlStore">The underlying Xml store.</param>
        /// <param name="token">The preview token.</param>
        public PreviewContent(XmlStore xmlStore, string token)
        {
            if (xmlStore == null)
                throw new ArgumentNullException(nameof(xmlStore));
            _xmlStore = xmlStore;

            if (token.IsNullOrWhiteSpace())
                throw new ArgumentException("Null or empty token.", nameof(token));
            var parts = token.Split(':');
            if (parts.Length != 2)
                throw new ArgumentException("Invalid token.", nameof(token));

            if (int.TryParse(parts[0], out _userId) == false)
                throw new ArgumentException("Invalid token.", nameof(token));
            if (Guid.TryParse(parts[1], out _previewSet) == false)
                throw new ArgumentException("Invalid token.", nameof(token));

            _previewSetPath = GetPreviewSetPath(_userId, _previewSet);
        }

        // creates and saves a new preview set
        // used in 2 places and each time includeSubs is true
        // have to use the Document class at the moment because IContent does not do ToXml...
        public void CreatePreviewSet(int contentId, bool includeSubs)
        {
            // note: always include subs
            _previewXml = _xmlStore.GetPreviewXml(contentId, includeSubs);

            // make sure the preview folder exists
            var dir = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Preview));
            if (dir.Exists == false)
                dir.Create();

            // clean old preview sets
            ClearPreviewDirectory(_userId, dir);

            // save
            _previewXml.Save(_previewSetPath);
        }

        // get the full path to the preview set
        private static string GetPreviewSetPath(int userId, Guid previewSet)
        {
            return IOHelper.MapPath(Path.Combine(SystemDirectories.Preview, userId + "_" + previewSet + ".config"));
        }

        // deletes files for the user, and files accessed more than one hour ago
        private static void ClearPreviewDirectory(int userId, DirectoryInfo dir)
        {
            var now = DateTime.Now;
            var prefix = userId + "_";
            foreach (var file in dir.GetFiles("*.config")
                .Where(x => x.Name.StartsWith(prefix) || (now - x.LastAccessTime).TotalMinutes > 1))
            {
                DeletePreviewSetFile(userId, file);
            }
        }

        // delete one preview set file in a safe way
        private static void DeletePreviewSetFile(int userId, FileSystemInfo file)
        {
            try
            {
                file.Delete();
            }
            catch (Exception ex)
            {
                Current.Logger.Error<PreviewContent, string,int>(ex, "Couldn't delete preview set {FileName} for user {UserId}", file.Name, userId);
            }
        }

        /// <summary>
        /// Deletes the preview set in a safe way.
        /// </summary>
        public void ClearPreviewSet()
        {
            if (_previewSetPath == null) return;
            var previewSetFile = new FileInfo(_previewSetPath);
            DeletePreviewSetFile(_userId, previewSetFile);
        }
    }
}
