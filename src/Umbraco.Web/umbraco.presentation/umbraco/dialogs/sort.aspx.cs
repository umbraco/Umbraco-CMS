using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;
using Umbraco.Core;
using Umbraco.Web;
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

        protected bool HideDateColumn
        {
            set { ViewState["HideDateColumn"] = value; }
            get { return ViewState["HideDateColumn"] == null ? false : (bool) ViewState["HideDateColumn"]; }
        }

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

            var app = Request.GetItemAsString("app");

            var icon = "../images/umbraco/doc.gif";

            int parentId;
            if (int.TryParse(Request.GetItemAsString("ID"), out parentId))
            {
                if (app == Constants.Applications.Media)
                {
                    icon = "../images/umbraco/mediaPhoto.gif";
                    var mediaService = ApplicationContext.Current.Services.MediaService;

                    if (parentId == -1)
                    {
                        foreach (var child in mediaService.GetRootMedia().ToList().OrderBy(x => x.SortOrder))
                            _nodes.Add(CreateNode(child.Id.ToInvariantString(), child.SortOrder, child.Name, child.CreateDate, icon));
                    }
                    else
                    {
                        var children = mediaService.GetChildren(parentId);
                        foreach (var child in children.OrderBy(x => x.SortOrder))
                            _nodes.Add(CreateNode(child.Id.ToInvariantString(), child.SortOrder, child.Name, child.CreateDate, icon));
                    }
                }

                if (app == Constants.Applications.Content)
                {
                    var contentService = ApplicationContext.Current.Services.ContentService;

                    if (parentId == -1)
                    {
                        foreach (var child in contentService.GetRootContent().ToList().OrderBy(x => x.SortOrder))
                            _nodes.Add(CreateNode(child.Id.ToInvariantString(), child.SortOrder, child.Name, child.CreateDate, icon));
                    }
                    else
                    {
                        var children = contentService.GetChildren(parentId);
                        foreach (var child in children)
                            _nodes.Add(CreateNode(child.Id.ToInvariantString(), child.SortOrder, child.Name, child.CreateDate, icon));
                    }
                }
                
                bindNodesToList(string.Empty);
            }
            else
            {
                // hack for stylesheet, used to sort stylesheet properties
                if (app == Constants.Applications.Settings)
                {
                    icon = "../images/umbraco/settingCss.gif";

                    HideDateColumn = true;

                    var stylesheetName = Request.GetItemAsString("ID");
                    if (stylesheetName.IsNullOrWhiteSpace())throw new NullReferenceException("No Id passed in to editor");
                    var stylesheet = Services.FileService.GetStylesheetByName(stylesheetName.EnsureEndsWith(".css"));
                    if (stylesheet == null) throw new InvalidOperationException("No stylesheet found by name " + stylesheetName);

                    var sort = 0;
                    foreach (var child in stylesheet.Properties)
                    {                        
                        _nodes.Add(CreateNode(child.Name, sort, child.Name, DateTime.Now, icon));
                        sort++;
                    }
                    
                    bindNodesToList(string.Empty);
                }
            }
        }

        public void bindNodesToList(string sortBy)
        {
            if (string.IsNullOrEmpty(sortBy) == false)
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

            foreach (var n in _nodes)
                lt_nodes.Text += string.Format(
                    "<tr id='node_{0}'><td>{1}</td><td class='nowrap' style='display:{5};'>{2} {3}</td><td style='text-align: center;'>{4}</td></tr>",
                    n.id, n.Name, n.createDate.ToShortDateString(), n.createDate.ToShortTimeString(), n.sortOrder, HideDateColumn ? "none" : "table-cell");
        }

        private static SortableNode CreateNode(string id, int sortOrder, string name, DateTime createDateTime, string icon)
        {
            var node = new SortableNode
                            {
                                id = id,
                                sortOrder = sortOrder,
                                Name = name,
                                icon = icon,
                                createDate = createDateTime
                            };
            return node;
        }

        public struct SortableNode
        {
            public string id;
            public int sortOrder;
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
            var returnValue = String.Compare(x.Name, y.Name, StringComparison.Ordinal);

            return returnValue;
        }

        #endregion
    }

    public class createDateCompare : IComparer<sort.SortableNode>
    {

        #region IComparer<CMSNode> Members

        public int Compare(sort.SortableNode x, sort.SortableNode y)
        {
            var returnValue = x.createDate.CompareTo(y.createDate);

            return returnValue;
        }

        #endregion
    }
}