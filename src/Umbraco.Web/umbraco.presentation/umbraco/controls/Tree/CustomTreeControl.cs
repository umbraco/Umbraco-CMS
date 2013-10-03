using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using ClientDependency.Core;
using umbraco.controls.Tree;
using Umbraco.Core.IO;

namespace umbraco.controls.Tree
{
    /// <summary>
    /// A custom tree control that uses a custom web service to return the initial node, this is required
    /// due to a bug that exists in Umbraco 4.5.1 tree control/web service.
    /// </summary>
    /// <remarks>
    /// Since we're inheriting from a UserControl and all of the ClientDependency registrations are done inline, we need
    /// to re-register the ClientDependencies.
    /// </remarks>
    [ClientDependency(10, ClientDependencyType.Css, "Tree/treeIcons.css", "UmbracoClient")]
    [ClientDependency(11, ClientDependencyType.Css, "Tree/menuIcons.css", "UmbracoClient")]
    [ClientDependency(12, ClientDependencyType.Css, "Tree/Themes/umbraco/style.css", "UmbracoClient")]
    [ClientDependency(0, ClientDependencyType.Javascript, "Application/NamespaceManager.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "Application/UmbracoClientManager.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "Application/UmbracoApplicationActions.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "Application/UmbracoUtils.js", "UmbracoClient")]
    [ClientDependency(0, ClientDependencyType.Javascript, "ui/jquery.js", "UmbracoClient")]
    [ClientDependency(10, ClientDependencyType.Javascript, "Application/JQuery/jquery.metadata.min.js", "UmbracoClient")]
    [ClientDependency(11, ClientDependencyType.Javascript, "Tree/jquery.tree.js", "UmbracoClient")]
    [ClientDependency(12, ClientDependencyType.Javascript, "Tree/UmbracoContext.js", "UmbracoClient")]
    [ClientDependency(12, ClientDependencyType.Javascript, "Tree/jquery.tree.contextmenu.js", "UmbracoClient")]
    [ClientDependency(12, ClientDependencyType.Javascript, "Tree/jquery.tree.checkbox.js", "UmbracoClient")]
    [ClientDependency(12, ClientDependencyType.Javascript, "Tree/NodeDefinition.js", "UmbracoClient")]
    [ClientDependency(13, ClientDependencyType.Javascript, "Tree/UmbracoTree.js", "UmbracoClient")]
    public class CustomTreeControl : TreeControl
    {
        private static readonly object m_Locker = new object();

        /// <summary>
        /// Ensure child controls are created on  init
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.EnsureChildControls();
        }

        /// <summary>
        /// Create the child controls
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            TreeContainer = new HtmlGenericControl();
            TreeContainer.TagName = "div";
            TreeContainer.ID = "TreeContainer";

            this.Controls.Add(TreeContainer);
        }

        /// <summary>
        /// Adds the internal markup to the TreeContainer control
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //add the internal markup to the TreeContainer
            /* <div id="<%=ClientID%>" class="<%#Mode.ToString().ToLower()%>"></div>  */
            TreeContainer.Controls.Add(new LiteralControl(@"<div id=""" + ClientID + @""" class=""" + Mode.ToString().ToLower() + @"""></div>"));
        }

        /// <summary>
        /// Render out the correct markup for the tree
        /// </summary>
        /// <remarks>
        /// Since we're inheriting from a UserControl, we need to render out the markup manually
        /// </remarks>
        /// <param name="writer"></param>
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            //You'll notice that we're replacing the 'serviceUrl' parameter with our own custom web service!

            writer.Write(@"
<script type=""text/javascript"">
jQuery(document).ready(function() {
    var ctxMenu = " + GetJSONContextMenu() + @";	
    var app = """ + App + @""";
    var showContext = " + ShowContextMenu.ToString().ToLower() + @";
    var isDialog = " + IsDialog.ToString().ToLower() + @";
    var dialogMode = """ + DialogMode.ToString() + @""";
    var treeType = """ + TreeType + @""";
    var functionToCall = """ + FunctionToCall + @""";
    var nodeKey = """ + NodeKey + @""";
	
    //create the javascript tree
    jQuery(""#" + ClientID + @""").UmbracoTree({
        doNotInit: " + ManualInitialization.ToString().ToLower() + @",
        jsonFullMenu: ctxMenu,
        appActions: UmbClientMgr.appActions(),
        deletingText: '" + umbraco.ui.GetText("deleting") + @"',
        app: app,
        showContext: showContext,
        isDialog: isDialog,
        dialogMode: dialogMode,
        treeType: treeType,
        functionToCall : functionToCall,
        nodeKey : nodeKey,
        treeMode: """ + Mode.ToString().ToLower() + @""",
        dataUrl: """ + IOHelper.ResolveUrl(SystemDirectories.Umbraco) + @"/webservices/TreeDataService.ashx"",
        serviceUrl: """ + IOHelper.ResolveUrl(SystemDirectories.Umbraco) + @"/controls/Tree/CustomTreeService.asmx/GetInitAppTreeData""});
        
     //add event handler for ajax errors, this will refresh the whole application
    var mainTree = UmbClientMgr.mainTree();
    if (mainTree != null) {
        mainTree.addEventHandler(""ajaxError"", function(e) {
            if (e.msg == ""rebuildTree"") {
	            UmbClientMgr.mainWindow(""umbraco.aspx"");
            }
        });
    }
});	

</script>");

            //render the controls
            TreeContainer.RenderControl(writer);
        }
    }
}