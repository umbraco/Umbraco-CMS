using Umbraco.Core.Services;
using System;
using System.Web.UI;
using Umbraco.Core;
using umbraco.cms.presentation.Trees;
using Umbraco.Core.IO;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.UI.Pages;

namespace umbraco.cms.presentation.user
{

	public partial class PermissionEditor : UmbracoEnsuredPage
    {
	    public PermissionEditor()
	    {
            CurrentApp = Constants.Applications.Users;

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

            CheckUser(Request.QueryString["id"]);

            var save = pnlUmbraco.Menu.NewButton();
            save.ID = "btnSave";
            save.ButtonType = uicontrols.MenuButtonType.Primary;
            save.OnClientClick = "SavePermissions(); return false;";
            save.Text = Services.TextService.Localize("save");
            save.ToolTip = Services.TextService.Localize("save");


            nodePermissions.UserID = Convert.ToInt32(Request.QueryString["id"]);
            pnlUmbraco.Text = Services.TextService.Localize("user/userPermissions");
            pnl1.Text = Services.TextService.Localize("user/permissionSelectPages");

			if (!IsPostBack)
			{	
				ClientTools cTools = new ClientTools(this);
				cTools.SetActiveTreeType(Constants.Trees.UserPermissions)
					.SyncTree(Request.QueryString["id"], false);
			}
        }

        /// <summary>
        /// Since Umbraco stores users in cache, we'll use this method to retrieve our user object by the selected id
        /// </summary>
        public IUser UmbracoUser
        {
            get
            {
                return Services.UserService.GetUserById(Convert.ToInt32(Request.QueryString["id"]));
            }
        }
      
        /// <summary>
        /// Makes sure the user exists with the id specified
        /// </summary>
        /// <param name="strID"></param>
        private void CheckUser(string strID)
        {
            int id;
            bool parsed = false;
            IUser oUser = null;
            if (parsed = int.TryParse(strID, out id))
                oUser = Services.UserService.GetUserById(id);

            if (oUser == null || oUser.UserType == null || parsed == false)
                throw new Exception("No user found with id: " + strID);
        }

       
        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference(SystemDirectories.Umbraco + "/users/PermissionsHandler.asmx"));
        }

    }
}
