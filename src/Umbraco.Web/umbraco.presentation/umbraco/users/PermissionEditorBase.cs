using System;
using System.Web.UI;
using Umbraco.Core;
using umbraco.BasePages;
using umbraco.cms.presentation.Trees;
using Umbraco.Core.IO;

namespace umbraco.cms.presentation.user
{

    public abstract class PermissionEditorBase : UmbracoEnsuredPage
    {
        #region Controls

        protected global::ClientDependency.Core.Controls.CssInclude CssInclude2;

        protected global::ClientDependency.Core.Controls.CssInclude CssInclude1;

        protected global::ClientDependency.Core.Controls.JsInclude JsInclude1;

        protected global::umbraco.uicontrols.UmbracoPanel pnlUmbraco;

        protected global::umbraco.uicontrols.Pane pnl1;

        protected global::umbraco.controls.Tree.TreeControl JTree;

        #endregion

        protected PermissionEditorBase()
        {
            CurrentApp = BusinessLogic.DefaultApps.users.ToString();
        }

        public abstract string PersmissionsHandlerServiceName { get; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!IsPostBack)
            {
                JTree.App = Constants.Applications.Content;
                JTree.ShowContextMenu = false;
                JTree.IsDialog = true;
            }
        }

        protected void InitControls()
        {
            var save = pnlUmbraco.Menu.NewButton();
            save.ID = "btnSave";
            save.ButtonType = uicontrols.MenuButtonType.Primary;
            save.OnClientClick = "SavePermissions(); return false;";
            save.Text = ui.Text("save");
            save.ToolTip = ui.Text("save");

            pnl1.Text = ui.Text("user", "permissionSelectPages");
        }

        protected void InitTree(TreeDefinition treeDefinition)
        {
            if (!IsPostBack)
            {
                var cTools = new ClientTools(this);
                cTools.SetActiveTreeType(treeDefinition.Tree.Alias)
                    .SyncTree(Request.QueryString["id"], false);
            }
        }

        /// <summary>
        /// Since Umbraco stores users in cache, we'll use this method to retrieve our user object by the selected id
        /// </summary>
        protected umbraco.BusinessLogic.User UmbracoUser
        {
            get
            {
                return BusinessLogic.User.GetUser(Convert.ToInt32(Request.QueryString["id"]));
            }
        }

        protected abstract void CheckEntity(string strID);

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference(SystemDirectories.Umbraco + "/users/" + PersmissionsHandlerServiceName + ".asmx"));
        }
    }
}
