using System;
using System.Web.UI;
using umbraco.BasePages;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Membership;

namespace umbraco.cms.presentation.user
{
    public partial class PermissionEditor : UmbracoEnsuredPage
    {
	    public PermissionEditor()
	    {
            CurrentApp = BusinessLogic.DefaultApps.users.ToString();

	    }

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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Request.QueryString["id"]))
                return;

            CheckUserGroup(Request.QueryString["id"]);

            InitControls();
            InitTree();
        }

        private void InitControls()
        {
            var save = pnlUmbraco.Menu.NewButton();
            save.ID = "btnSave";
            save.ButtonType = uicontrols.MenuButtonType.Primary;
            save.OnClientClick = "SavePermissions(); return false;";
            save.Text = ui.Text("save");
            save.ToolTip = ui.Text("save");

            pnl1.Text = ui.Text("user", "permissionSelectPages");
        }

	    private void InitTree()
	    {
	        if (!IsPostBack)
	        {
	            ClientTools cTools = new ClientTools(this);
	            cTools.SetActiveTreeType(
	                TreeDefinitionCollection.Instance.FindTree<Trees.UserGroupPermissions>().Tree.Alias)
	                .SyncTree(Request.QueryString["id"], false);
	        }
	    }
      
        /// <summary>
        /// Makes sure the user group exists with the id specified
        /// </summary>
        /// <param name="strID"></param>
        private void CheckUserGroup(string strID)
        {
            int id;
            bool parsed = false;
            IUserGroup group = null;
            if (parsed = int.TryParse(strID, out id))
            {
                var userService = ApplicationContext.Current.Services.UserService;
                group = userService.GetUserGroupById(id);
            }

            if (group == null || parsed == false)
            {
                throw new Exception("No group found with id: " + strID);
            }

            nodePermissions.UserGroupID = id;
        }

       
        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference(SystemDirectories.Umbraco + "/users/PermissionsHandler.asmx"));
        }

    }
}
