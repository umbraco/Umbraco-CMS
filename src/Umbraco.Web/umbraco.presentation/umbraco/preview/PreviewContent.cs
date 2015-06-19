using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using Umbraco.Core.IO;

namespace umbraco.presentation.preview
{
    //TODO : Migrate this to a new API!

    public class PreviewContent
    {
        // zb-00004 #29956 : refactor cookies names & handling

        public XmlDocument XmlContent { get; set; }
        public Guid PreviewSet { get; set; }
        public string PreviewsetPath { get; set; }

        public bool ValidPreviewSet { get; set; }

        private int _userId = -1;

        public PreviewContent()
        {
            _initialized = false;
        }

        private readonly object _initLock = new object();
        private bool _initialized = true;

        public void EnsureInitialized(User user, string previewSet, bool validate, Action initialize)
        {
            lock (_initLock)
            {
                if (_initialized) return;

                _userId = user.Id;
                ValidPreviewSet = UpdatePreviewPaths(new Guid(previewSet), validate);
                initialize();
                _initialized = true;
            }
        }

        public PreviewContent(User user)
        {
            _userId = user.Id;
        }

        public PreviewContent(Guid previewSet)
        {
            ValidPreviewSet = UpdatePreviewPaths(previewSet, true);
        }
        public PreviewContent(User user, Guid previewSet, bool validate)
        {
            _userId = user.Id;
            ValidPreviewSet = UpdatePreviewPaths(previewSet, validate);
        }


        public void PrepareDocument(User user, Document documentObject, bool includeSubs)
        {
            _userId = user.Id;

            // clone xml
            XmlContent = (XmlDocument)content.Instance.XmlContent.Clone();

            var previewNodes = new List<Document>();

            var parentId = documentObject.Level == 1 ? -1 : documentObject.Parent.Id;

            while (parentId > 0 && XmlContent.GetElementById(parentId.ToString(CultureInfo.InvariantCulture)) == null)
            {
                var document = new Document(parentId);
                previewNodes.Insert(0, document);
                parentId = document.ParentId;
            }

            previewNodes.Add(documentObject);

            foreach (var document in previewNodes)
            {
                //Inject preview xml
                parentId = document.Level == 1 ? -1 : document.Parent.Id;
                var previewXml = document.ToPreviewXml(XmlContent);
                if (document.ContentEntity.Published == false 
                    && ApplicationContext.Current.Services.ContentService.HasPublishedVersion(document.Id))
                    previewXml.Attributes.Append(XmlContent.CreateAttribute("isDraft"));
                content.AddOrUpdateXmlNode(XmlContent, document.Id, document.Level, parentId, previewXml);
            }

            if (includeSubs)
            {
                foreach (var prevNode in documentObject.GetNodesForPreview(true))
                {
                    var previewXml = XmlContent.ReadNode(XmlReader.Create(new StringReader(prevNode.Xml)));
                    if (prevNode.IsDraft)
                        previewXml.Attributes.Append(XmlContent.CreateAttribute("isDraft"));
                    content.AddOrUpdateXmlNode(XmlContent, prevNode.NodeId, prevNode.Level, prevNode.ParentId, previewXml);
                }
            }

        }

        private bool UpdatePreviewPaths(Guid previewSet, bool validate)
        {
            if (_userId == -1)
            {
                throw new ArgumentException("No current Umbraco User registered in Preview", "m_userId");
            }

            PreviewSet = previewSet;
            PreviewsetPath = GetPreviewsetPath(_userId, previewSet);

            if (validate && !ValidatePreviewPath())
            {
                // preview cookie failed so we'll log the error and clear the cookie
                LogHelper.Debug<PreviewContent>(string.Format("Preview failed for preview set {0} for user {1}", previewSet, _userId));

                PreviewSet = Guid.Empty;
                PreviewsetPath = String.Empty;

                ClearPreviewCookie();

                return false;
            }

            return true;
        }

        private static string GetPreviewsetPath(int userId, Guid previewSet)
        {
            return IOHelper.MapPath(
                Path.Combine(SystemDirectories.Preview, userId + "_" + previewSet + ".config"));
        }

        /// <summary>
        /// Checks a preview file exist based on preview cookie
        /// </summary>
        /// <returns></returns>
        public bool ValidatePreviewPath()
        {
            if (!File.Exists(PreviewsetPath))
                return false;

            return true;
        }



        public void LoadPreviewset()
        {
            XmlContent = new XmlDocument();
            XmlContent.Load(PreviewsetPath);
        }

        public void SavePreviewSet()
        {
            //make sure the preview folder exists first
            var dir = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Preview));
            if (!dir.Exists)
            {
                dir.Create();
            }

            // check for old preview sets and try to clean
            CleanPreviewDirectory(_userId, dir);

            XmlContent.Save(PreviewsetPath);
        }

        private static void CleanPreviewDirectory(int userId, DirectoryInfo dir)
        {
            foreach (FileInfo file in dir.GetFiles(userId + "_*.config"))
            {
                DeletePreviewFile(userId, file);
            }
            // also delete any files accessed more than one hour ago
            foreach (FileInfo file in dir.GetFiles("*.config"))
            {
                if ((DateTime.Now - file.LastAccessTime).TotalMinutes > 1)
                    DeletePreviewFile(userId, file);
            }
        }

        private static void DeletePreviewFile(int userId, FileInfo file)
        {
            try
            {
                file.Delete();
            }
            catch (Exception ex)
            {
                LogHelper.Error<PreviewContent>(string.Format("Couldn't delete preview set: {0} - User {1}", file.Name, userId), ex);
            }
        }

        public void ActivatePreviewCookie()
        {
            // zb-00004 #29956 : refactor cookies names & handling
            StateHelper.Cookies.Preview.SetValue(PreviewSet.ToString());
        }

        public static void ClearPreviewCookie()
        {
            // zb-00004 #29956 : refactor cookies names & handling
            if (UmbracoContext.Current.UmbracoUser != null)
            {
                if (StateHelper.Cookies.Preview.HasValue)
                {

                    DeletePreviewFile(
                        UmbracoContext.Current.UmbracoUser.Id,
                        new FileInfo(GetPreviewsetPath(
                            UmbracoContext.Current.UmbracoUser.Id,
                            new Guid(StateHelper.Cookies.Preview.GetValue()))));
                }
            }
            StateHelper.Cookies.Preview.Clear();
        }
    }
}
