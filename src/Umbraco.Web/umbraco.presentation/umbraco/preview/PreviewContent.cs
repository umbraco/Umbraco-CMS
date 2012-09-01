using System;
using System.Data;
using System.Configuration;
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
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;

namespace umbraco.presentation.preview
{
    public class PreviewContent
    {
        // zb-00004 #29956 : refactor cookies names & handling

        public XmlDocument XmlContent { get; set; }
        public Guid PreviewSet { get; set; }
        public string PreviewsetPath { get; set; }

        public bool ValidPreviewSet { get; set; }

        private int _userId = -1;

        public PreviewContent(User user)
        {
            _userId = user.Id;
        }

        public PreviewContent(Guid previewSet)
        {
            ValidPreviewSet = updatePreviewPaths(previewSet, true);
        }
        public PreviewContent(User user, Guid previewSet, bool validate)
        {
            _userId = user.Id;
            ValidPreviewSet = updatePreviewPaths(previewSet, validate);
        }


        public void PrepareDocument(User user, Document documentObject, bool includeSubs)
        {
            _userId = user.Id;

            // clone xml
            XmlContent = (XmlDocument)content.Instance.XmlContent.Clone();

            // inject current document xml
            int parentId = documentObject.Level == 1 ? -1 : documentObject.Parent.Id;
            content.AppendDocumentXml(documentObject.Id, documentObject.Level, parentId, documentObject.ToPreviewXml(XmlContent), XmlContent);

            if (includeSubs)
            {
                foreach (CMSPreviewNode prevNode in documentObject.GetNodesForPreview(true))
                {
                    content.AppendDocumentXml(prevNode.NodeId, prevNode.Level, prevNode.ParentId, XmlContent.ReadNode(XmlReader.Create(new StringReader(prevNode.Xml))), XmlContent);
                }
            }

        }

        private bool updatePreviewPaths(Guid previewSet, bool validate)
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
                Log.Add(LogTypes.Error, User.GetUser(_userId), -1, string.Format("Preview failed for preview set {0}", previewSet));
                PreviewSet = Guid.Empty;
                PreviewsetPath = String.Empty;

                ClearPreviewCookie();

                return false;
            }

            return true;
        }

        private static string GetPreviewsetPath(int userId, Guid previewSet)
        {
            return IO.IOHelper.MapPath(
                Path.Combine(IO.SystemDirectories.Preview, userId + "_" + previewSet + ".config"));
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
            var dir = new DirectoryInfo(IO.IOHelper.MapPath(IO.SystemDirectories.Preview));
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
                deletePreviewFile(userId, file);
            }
            // also delete any files accessed more than one hour ago
            foreach (FileInfo file in dir.GetFiles("*.config"))
            {
                if ((DateTime.Now - file.LastAccessTime).TotalMinutes > 1)
                    deletePreviewFile(userId, file);
            }
        }

        private static void deletePreviewFile(int userId, FileInfo file)
        {
            try
            {
                file.Delete();
            }
            catch
            {
                Log.Add(LogTypes.Error, User.GetUser(userId), -1, String.Format("Couldn't delete preview set: {0}", file.Name));
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

                    deletePreviewFile(
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
