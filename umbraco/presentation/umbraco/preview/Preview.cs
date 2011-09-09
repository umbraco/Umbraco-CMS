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
        
        private int m_userId = 0;

        public PreviewContent(User user)
        {
            m_userId = user.Id;
        }

        public void PrepareDocument(User user, Document documentObject, bool includeSubs)
        {
            m_userId = user.Id;

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

        public PreviewContent(Guid previewSet)
        {
            ValidPreviewSet = updatePreviewPaths(previewSet);
        }

        private bool updatePreviewPaths(Guid previewSet)
        {
            PreviewSet = previewSet;
            PreviewsetPath = IO.IOHelper.MapPath(
                Path.Combine(IO.SystemDirectories.Preview, m_userId.ToString() + "_" + PreviewSet + ".config"));

            if (!ValidatePreviewPath())
            {
                // preview cookie failed so we'll log the error and clear the cookie
                Log.Add(LogTypes.Error, User.GetUser(m_userId), -1, string.Format("Preview failed for preview set {0}", previewSet));
                PreviewSet = Guid.Empty;
                PreviewsetPath = String.Empty;

                ClearPreviewCookie();

                return false;
            }

            return true;
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
            foreach (FileInfo file in dir.GetFiles(m_userId + "_*.config"))
            {
                try
                {
                    file.Delete();
                }
                catch {
                    Log.Add(LogTypes.Error, User.GetUser(m_userId), -1, String.Format("Couldn't delete preview set: {0}", file.Name));
                }
            }

            XmlContent.Save(PreviewsetPath);
        }

        public void ActivatePreviewCookie() {
			// zb-00004 #29956 : refactor cookies names & handling
			StateHelper.Cookies.Preview.SetValue(PreviewSet.ToString());
        }

        public static void ClearPreviewCookie() {
			// zb-00004 #29956 : refactor cookies names & handling
			StateHelper.Cookies.Preview.Clear();
        }
    }
}
