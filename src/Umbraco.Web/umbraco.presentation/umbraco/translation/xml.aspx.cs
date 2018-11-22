using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Xml;
using System.Xml.Schema;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.task;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.IO;

namespace umbraco.presentation.translation
{
    public partial class xml : BasePages.UmbracoEnsuredPage
    {
        private readonly XmlDocument _xd = new XmlDocument();

        public xml()
        {
            CurrentApp = DefaultApps.translation.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "text/xml";
            int pageId;

            XmlNode root = _xd.CreateElement("tasks");

            if (int.TryParse(Request["id"], out pageId))
            {
                var t = new Task(pageId);
                if (t.User.Id == base.getUser().Id || t.ParentUser.Id == base.getUser().Id)
                {
                    XmlNode x = CreateTaskNode(t, _xd);
                    root.AppendChild(x);

                    xmlContents.Text = root.OuterXml;

                    Response.AddHeader("Content-Disposition", "attachment; filename=" + t.Node.Text.Replace(" ", "_") + ".xml");
                }
            }
            else
            {
                var nodes = new SortedList();
                int totalWords = 0;

                foreach (Task t in Task.GetTasks(base.getUser(), false))
                {
                    if (!nodes.ContainsKey(t.Node.Path))
                    {
                        var xTask = CreateTaskNode(t, _xd);
                        totalWords += int.Parse(xTask.Attributes.GetNamedItem("TotalWords").Value);
                        nodes.Add(t.Node.Path, xTask);
                    }
                }

                // Arrange nodes in tree
                var ide = nodes.GetEnumerator();
                while (ide.MoveNext())
                {
                    var x = (XmlElement)ide.Value;
                    var parentXpath = UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema ? "//node [@id = '" + x.SelectSingleNode("//node").Attributes.GetNamedItem("parentID").Value + "']" :
                        "//* [@isDoc and @id = '" + x.SelectSingleNode("//* [@isDoc]").Attributes.GetNamedItem("parentID").Value + "']";
                    var parent = _xd.SelectSingleNode(parentXpath);

                    if (parent == null)
                        parent = root;
                    else
                        parent = parent.ParentNode;

                    parent.AppendChild((XmlElement)ide.Value);
                }

                root.Attributes.Append(XmlHelper.AddAttribute(_xd, "TotalWords", totalWords.ToString()));
                xmlContents.Text = root.OuterXml;
                Response.AddHeader("Content-Disposition", "attachment; filename=all.xml");

            }
        }

        private XmlElement CreateTaskNode(Task t, XmlDocument xd)
        {
            var d = new Document(t.Node.Id);
            var x = d.ToPreviewXml(xd);//  xd.CreateNode(XmlNodeType.Element, "node", "");

            var xTask = xd.CreateElement("task");
            xTask.SetAttributeNode(XmlHelper.AddAttribute(xd, "Id", t.Id.ToString()));
            xTask.SetAttributeNode(XmlHelper.AddAttribute(xd, "Date", t.Date.ToString("s")));
            xTask.SetAttributeNode(XmlHelper.AddAttribute(xd, "NodeId", t.Node.Id.ToString()));
            xTask.SetAttributeNode(XmlHelper.AddAttribute(xd, "TotalWords", cms.businesslogic.translation.Translation.CountWords(d.Id).ToString()));
            xTask.AppendChild(XmlHelper.AddCDataNode(xd, "Comment", t.Comment));
            string protocol = GlobalSettings.UseSSL ? "https" : "http";
            xTask.AppendChild(XmlHelper.AddTextNode(xd, "PreviewUrl", protocol + "://" + Request.ServerVariables["SERVER_NAME"] + SystemDirectories.Umbraco + "/translation/preview.aspx?id=" + t.Id.ToString()));
            //            d.XmlPopulate(xd, ref x, false);
            xTask.AppendChild(x);

            return xTask;
        }
    }
}
