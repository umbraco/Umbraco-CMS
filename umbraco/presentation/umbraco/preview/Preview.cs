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
        public XmlDocument XmlContent { get; set; }
        public Guid PreviewSet { get; set; }
        public string PreviewsetPath { get; set; }
        private int m_userId = 0;
        public PreviewContent(User user)
        {
            m_userId = user.Id;
        }

        public void PrepareDocument(User user, Document documentObject, bool includeSubs)
        {
            m_userId = user.Id;

            // clone xml
            XmlContent = (XmlDocument) content.Instance.XmlContent.Clone();
            
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
            updatePreviewPaths(previewSet);
        }

        private void updatePreviewPaths(Guid previewSet)
        {
            PreviewSet = previewSet;
            PreviewsetPath = IO.IOHelper.MapPath(
                Path.Combine(
                    Path.Combine(IO.SystemDirectories.Data, "previews"),
                    PreviewSet + ".config"));
        }

        public void LoadPreviewset()
        {
            XmlContent = new XmlDocument();
            XmlContent.Load(PreviewsetPath);
        }

        public void SavePreviewSet()
        {
            XmlContent.Save(PreviewsetPath);
        }
    }
}
