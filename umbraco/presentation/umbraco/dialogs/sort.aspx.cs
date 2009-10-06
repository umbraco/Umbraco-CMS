using System;
using System.Collections;
using System.Web.UI.WebControls;
using System.Xml;
using umbraco.BasePages;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.web;
using System.Web.UI;
using System.Collections.Generic;

namespace umbraco.cms.presentation
{
    /// <summary>
    /// Summary description for sort.
    /// </summary>
    public partial class sort : UmbracoEnsuredPage
    {
        private int parentId;
        private List<SortableNode> _nodes = new List<SortableNode>();

        protected void Page_Load(object sender, EventArgs e)
        {
            parentId = int.Parse(Request.QueryString["id"]);

            // The following code isn't processed as we're using ajax call with a webservice now
            if (Request.QueryString["save"] != null)
            {
                string tmpVal = Request.QueryString["save"];
                if (tmpVal.Trim().Length > 0)
                {
                    string[] tmp = tmpVal.Split(',');

                    for (int i = 0; i < tmp.Length; i++)
                    {
                        new CMSNode(int.Parse(tmp[i])).sortOrder = i;
                        if (helper.Request("app") == "content")
                        {
                            Document d = new Document(int.Parse(tmp[i]));
                            // refresh the xml for the sorting to work
                            if (d.Published)
                            {
                                d.refreshXmlSortOrder();
                                library.PublishSingleNode(int.Parse(tmp[i]));
                            }
                        }
                    }

                    // Republish
                    if (helper.Request("app") == "content")
                    {
                        // Re-sort
                        XmlNode n;
                        if (parentId > 0)
                            n = content.Instance.XmlContent.GetElementById(parentId.ToString());
                        else
                            n = content.Instance.XmlContent.DocumentElement;

                        content.SortNodes(ref n);

                        // Run ActionHandler
                        if (parentId > 0)
                            umbraco.BusinessLogic.Actions.Action.RunActionHandlers(new Document(parentId), ActionSort.Instance);

                        //PPH Refresh content cache (for load-balancing)
                        library.RefreshContent();
                    }
                }
				BasePage.Current.ClientTools.ReloadActionNode(true, true);
            }

            sortDone.Text = ui.Text("sort","sortDone");
           
        }

        private void populateListBox()
        {
            /*
            ListBox1.Items.Clear();
            ArrayList tmp = new ArrayList();

            if (parentId > 1)
            {
                if (helper.Request("app") == "settings")
                {
                    // stylesheets
                    foreach (StylesheetProperty p in new StyleSheet(parentId).Properties)
                    {
                        tmp.Add(new ListItem(p.Text, p.Id.ToString()));
                    }
                }
                else
                {
                    foreach (Document d in new Document(parentId).Children)
                    {
                        tmp.Add(new ListItem(d.Text, d.Id.ToString()));
                    }
                }
            }
            else
            {
                //get all topmost nodes
                if (helper.Request("app") == "media")
                {
                    foreach (Media m in  Media.GetRootMedias())
                    {
                        tmp.Add(new ListItem(m.Text, m.Id.ToString()));
                    }
                }
                else
                {
                    foreach (Document d in  Document.GetRootDocuments())
                    {
                        tmp.Add(new ListItem(d.Text, d.Id.ToString()));
                    }
                }
            }

            for (int i = 0; i < tmp.Count; i++)
            {
                ListBox1.Items.Add((ListItem) tmp[i]);
            }
             */
        }

        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/nodesorter.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));

            int ParentId = 0;
            string App = umbraco.helper.Request("app");
            string icon = "../images/umbraco/doc.gif";

            if (int.TryParse(umbraco.helper.Request("ID"), out ParentId)) {
                
                if (ParentId == -1) {

                    if (App == "media") {
                        icon = "../images/umbraco/mediaPhoto.gif";
                        foreach (cms.businesslogic.media.Media child in cms.businesslogic.media.Media.GetRootMedias())
                            _nodes.Add(createNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime, icon));

                    } else {
                        foreach (cms.businesslogic.web.Document child in cms.businesslogic.web.Document.GetRootDocuments()) {
                            _nodes.Add(createNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime, icon));
                        }                        
                    }
                } else {
                    // "hack for stylesheet"
                    cms.businesslogic.CMSNode n = new cms.businesslogic.CMSNode(ParentId);
                    if (App == "settings") {
                        icon = "../images/umbraco/settingCss.gif";
                        StyleSheet ss = new StyleSheet(n.Id);
                        foreach (cms.businesslogic.web.StylesheetProperty child in ss.Properties)
                            _nodes.Add(createNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime, icon));

                    } else {
                        //store children array here because iterating over an Array property object is very inneficient.
                        var children = n.Children;
                        foreach (cms.businesslogic.CMSNode child in children)
                            _nodes.Add(createNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime, icon));
                    }
                }

                bindNodesToList(string.Empty);
            }
        }

        public void bindNodesToList(string sortBy) {
                   
            if (!string.IsNullOrEmpty(sortBy)) {
                switch (sortBy) {
                    case "nodeName":
                        _nodes.Sort(new nodeNameCompare());
                        break;
                    case "createDate":
                        _nodes.Sort(new createDateCompare());
                        break;
                    default:
                        break;
                }
            }

           //lt_nodes.Text = "";

            foreach (SortableNode n in _nodes) {
                lt_nodes.Text += "<tr id='node_" + n.id.ToString() + "'><td>" + n.Name + "</td><td class='nowrap'>" + n.createDate.ToShortDateString() + " " + n.createDate.ToShortTimeString() + "</td><td style='text-align: center;'>" + n.sortOder + "</td></tr>";
            }
            
        }

        private static SortableNode createNode(int id, int sortOrder, string name, DateTime createDateTime, string icon) {
            SortableNode _node = new SortableNode();
            _node.id = id;
            _node.sortOder = sortOrder;
            _node.Name = name;
            _node.icon = icon;
            _node.createDate = createDateTime;
            return _node;
        }

        public struct SortableNode {
            public int id;
            public int sortOder;
            public string Name;
            public string icon;
            public DateTime createDate;
        }


        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        #endregion
    }

    public class nodeNameCompare : IComparer<sort.SortableNode> {

        #region IComparer<CMSNode> Members

        public int Compare(sort.SortableNode x, sort.SortableNode y) {
            int returnValue = 1;
           
                returnValue = x.Name.CompareTo(y.Name);
            
            return returnValue;
        }

        #endregion
    }

    public class createDateCompare : IComparer<sort.SortableNode> {

        #region IComparer<CMSNode> Members

        public int Compare(sort.SortableNode x, sort.SortableNode y) {
            int returnValue = 1;

                returnValue = x.createDate.CompareTo(y.createDate);
            

            return returnValue;
        }

        #endregion
    }
}