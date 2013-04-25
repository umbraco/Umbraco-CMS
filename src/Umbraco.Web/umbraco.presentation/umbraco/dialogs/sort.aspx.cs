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
        private readonly List<SortableNode> _nodes = new List<SortableNode>();

        protected override void OnInit(EventArgs e)
        {
            CurrentApp = helper.Request("app");

            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            sortDone.Text = ui.Text("sort", "sortDone");
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/nodesorter.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));

            var parentId = 0;

            var icon = "../images/umbraco/doc.gif";

            if (int.TryParse(helper.Request("ID"), out parentId))
            {

                if (parentId == -1)
                {

                    if (CurrentApp == "media")
                    {
                        icon = "../images/umbraco/mediaPhoto.gif";
                        foreach (var child in Media.GetRootMedias())
                            _nodes.Add(CreateNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime, icon));

                    }
                    else
                    {
                        foreach (var child in Document.GetRootDocuments())
                        {
                            _nodes.Add(CreateNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime, icon));
                        }
                    }
                }
                else
                {
                    // "hack for stylesheet"
                    var n = new CMSNode(parentId);
                    if (CurrentApp == "settings")
                    {
                        icon = "../images/umbraco/settingCss.gif";
                        var ss = new StyleSheet(n.Id);
                        foreach (var child in ss.Properties)
                            _nodes.Add(CreateNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime, icon));

                    }
                    else
                    {
                        //store children array here because iterating over an Array property object is very inneficient.
                        var children = n.Children;
                        foreach (CMSNode child in children)
                            _nodes.Add(CreateNode(child.Id, child.sortOrder, child.Text, child.CreateDateTime, icon));
                    }
                }

                bindNodesToList(string.Empty);
            }
        }

        public void bindNodesToList(string sortBy)
        {

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "nodeName":
                        _nodes.Sort(new nodeNameCompare());
                        break;
                    case "createDate":
                        _nodes.Sort(new createDateCompare());
                        break;
                }
            }

            //lt_nodes.Text = "";

            foreach (var n in _nodes)
            {
                lt_nodes.Text += "<tr id='node_" + n.id.ToString() + "'><td>" + n.Name + "</td><td class='nowrap'>" + n.createDate.ToShortDateString() + " " + n.createDate.ToShortTimeString() + "</td><td style='text-align: center;'>" + n.sortOder + "</td></tr>";
            }

        }

        private static SortableNode CreateNode(int id, int sortOrder, string name, DateTime createDateTime, string icon)
        {
            var node = new SortableNode();
            node.id = id;
            node.sortOder = sortOrder;
            node.Name = name;
            node.icon = icon;
            node.createDate = createDateTime;
            return node;
        }

        public struct SortableNode
        {
            public int id;
            public int sortOder;
            public string Name;
            public string icon;
            public DateTime createDate;
        }

        /// <summary>
        /// JsInclude1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude1;

        /// <summary>
        /// JsInclude2 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude2;

        /// <summary>
        /// prog1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.ProgressBar prog1;

        /// <summary>
        /// sortDone control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal sortDone;

        /// <summary>
        /// sortPane control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane sortPane;

        /// <summary>
        /// lt_nodes control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal lt_nodes;

    }

    public class nodeNameCompare : IComparer<sort.SortableNode>
    {

        #region IComparer<CMSNode> Members

        public int Compare(sort.SortableNode x, sort.SortableNode y)
        {
            int returnValue = 1;

            returnValue = x.Name.CompareTo(y.Name);

            return returnValue;
        }

        #endregion
    }

    public class createDateCompare : IComparer<sort.SortableNode>
    {

        #region IComparer<CMSNode> Members

        public int Compare(sort.SortableNode x, sort.SortableNode y)
        {
            int returnValue = 1;

            returnValue = x.createDate.CompareTo(y.createDate);


            return returnValue;
        }

        #endregion
    }
}