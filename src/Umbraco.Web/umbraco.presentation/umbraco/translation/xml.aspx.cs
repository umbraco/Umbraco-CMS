using System;
using System.Collections;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Core.Xml;
using Umbraco.Web.Composing;
using Umbraco.Web._Legacy.BusinessLogic;

namespace umbraco.presentation.translation
{
    public partial class xml : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {
        private readonly XmlDocument _xd = new XmlDocument();

        public xml()
        {
            CurrentApp = Constants.Applications.Translation.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.ContentType = "text/xml";
            int pageId;

            XmlNode root = _xd.CreateElement("tasks");

            if (int.TryParse(Request["id"], out pageId))
            {
                var t = new Task(pageId);
                if (t.User.Id == Security.CurrentUser.Id || t.ParentUser.Id == Security.CurrentUser.Id)
                {
                    XmlNode x = CreateTaskNode(t, _xd);
                    root.AppendChild(x);

                    xmlContents.Text = root.OuterXml;

                    Response.AddHeader("Content-Disposition", "attachment; filename=" + t.TaskEntityEntity.Name.Replace(" ", "_") + ".xml");
                }
            }
            else
            {
                var nodes = new SortedList();
                int totalWords = 0;

                foreach (Task t in Task.GetTasks(Security.CurrentUser, false))
                {
                    if (!nodes.ContainsKey(t.TaskEntityEntity.Path))
                    {
                        var xTask = CreateTaskNode(t, _xd);
                        totalWords += int.Parse(xTask.Attributes.GetNamedItem("TotalWords").Value);
                        nodes.Add(t.TaskEntityEntity.Path, xTask);
                    }
                }

                // Arrange nodes in tree
                var ide = nodes.GetEnumerator();
                while (ide.MoveNext())
                {
                    var x = (XmlElement)ide.Value;
                    var parentXpath = "//* [@isDoc and @id = '" + x.SelectSingleNode("//* [@isDoc]").Attributes.GetNamedItem("parentID").Value + "']";
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
            //var d = new Document(t.Node.Id);
            //var x = d.ToPreviewXml(xd);//  xd.CreateNode(XmlNodeType.Element, "node", "");

            var content = Current.Services.ContentService.GetById(t.TaskEntity.EntityId);

            const bool published = false; // no idea really
            var x = EntityXmlSerializer.Serialize(Current.Services.ContentService, Current.Services.DataTypeService, Current.Services.UserService, Current.Services.LocalizationService, Current.UrlSegmentProviders, content, published);


            var xTask = xd.CreateElement("task");
            xTask.SetAttributeNode(XmlHelper.AddAttribute(xd, "Id", t.Id.ToString()));
            xTask.SetAttributeNode(XmlHelper.AddAttribute(xd, "Date", t.Date.ToString("s")));
            xTask.SetAttributeNode(XmlHelper.AddAttribute(xd, "NodeId", t.TaskEntity.EntityId.ToString()));
            //TODO: Make this work again with correct APIs and angularized - so none of this code will exist anymore
            //xTask.SetAttributeNode(XmlHelper.AddAttribute(xd, "TotalWords", cms.businesslogic.translation.Translation.CountWords(d.Id).ToString()));
            xTask.AppendChild(XmlHelper.AddCDataNode(xd, "Comment", t.Comment));
            string protocol = UmbracoConfig.For.GlobalSettings().UseHttps ? "https" : "http";
            xTask.AppendChild(XmlHelper.AddTextNode(xd, "PreviewUrl", protocol + "://" + Request.ServerVariables["SERVER_NAME"] + SystemDirectories.Umbraco + "/translation/preview.aspx?id=" + t.Id.ToString()));
            //            d.XmlPopulate(xd, ref x, false);
            xTask.AppendChild(x.ToXmlElement());

            return xTask;
        }
    }
}
