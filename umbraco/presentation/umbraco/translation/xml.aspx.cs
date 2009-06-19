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
using umbraco.cms.businesslogic.task;
using umbraco.cms.businesslogic.web;

namespace umbraco.presentation.translation
{
    public partial class xml : BasePages.UmbracoEnsuredPage
    {
        private XmlDocument xd = new XmlDocument();
        
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "text/xml";
            int pageId;

            XmlNode root = xd.CreateElement("tasks");
                        
            if (int.TryParse(Request["id"], out pageId))
            {
                Task t = new Task(pageId);
                if (t.User.Id == base.getUser().Id || t.ParentUser.Id == base.getUser().Id) {
                    XmlNode x = CreateTaskNode(t, xd);
                    root.AppendChild(x);

                   xmlContents.Text = root.OuterXml;
                   Response.AddHeader("Content-Disposition", "attachment; filename=" + x.SelectSingleNode("//node").Attributes.GetNamedItem("nodeName").Value.Replace(" ", "_") + ".xml");
                }
            }
            else
            {
                SortedList nodes = new SortedList();
                int totalWords = 0;

                foreach (Task t in Task.GetTasks(base.getUser(), false))
                {
                    if (!nodes.ContainsKey(t.Node.Path)) {
                        XmlElement xTask = CreateTaskNode(t, xd);
                        totalWords += int.Parse(xTask.Attributes.GetNamedItem("TotalWords").Value);
                        nodes.Add(t.Node.Path, xTask);
                    }
                }

                // Arrange nodes in tree
                IDictionaryEnumerator ide = nodes.GetEnumerator();
                while (ide.MoveNext())
                {
                    XmlElement x = (XmlElement)ide.Value;
                    XmlNode parent = xd.SelectSingleNode("//node [@id = '" + x.SelectSingleNode("//node").Attributes.GetNamedItem("parentID").Value + "']");

                    if (parent == null)
                        parent = root;
                    else
                        parent = parent.ParentNode;

                    parent.AppendChild((XmlElement) ide.Value);
                }

                root.Attributes.Append(global::umbraco.xmlHelper.addAttribute(xd, "TotalWords" , totalWords.ToString() ) );
                xmlContents.Text = root.OuterXml;
                Response.AddHeader("Content-Disposition", "attachment; filename=all.xml");

            }
        }

        private XmlElement CreateTaskNode(Task t, XmlDocument xd)
        {
            Document d = new Document(t.Node.Id);
            XmlNode x = xd.CreateNode(XmlNodeType.Element, "node", "");

            XmlElement xTask = xd.CreateElement("task");
            xTask.SetAttributeNode(xmlHelper.addAttribute(xd, "Id", t.Id.ToString()));
            xTask.SetAttributeNode(xmlHelper.addAttribute(xd, "Date", t.Date.ToString("s")));
            xTask.SetAttributeNode(xmlHelper.addAttribute(xd, "NodeId", t.Node.Id.ToString()));
            xTask.SetAttributeNode(xmlHelper.addAttribute(xd, "TotalWords", cms.businesslogic.translation.Translation.CountWords(d.Id).ToString()));
            xTask.AppendChild(xmlHelper.addCDataNode(xd, "Comment", t.Comment));
            xTask.AppendChild(xmlHelper.addTextNode(xd, "PreviewUrl", "http://" + Request.ServerVariables["SERVER_NAME"] + GlobalSettings.Path + "/translation/preview.aspx?id=" + t.Id.ToString()));
            d.XmlPopulate(xd, ref x, false);
            xTask.AppendChild(x);

            return xTask;
        }
    }
}
