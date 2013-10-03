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
using umbraco.BasePages;
using System.Collections.Generic;
using umbraco.interfaces;
using umbraco.BusinessLogic.Actions;
using umbraco.BusinessLogic;
using umbraco.uicontrols;
using umbraco.cms.presentation.Trees;
using Umbraco.Core.IO;

namespace umbraco.cms.presentation.user
{
    public partial class EditUserType : UmbracoEnsuredPage
    {
        public EditUserType()
        {
            CurrentApp = BusinessLogic.DefaultApps.users.ToString();
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            pnlUmbraco.Text = umbraco.ui.Text("usertype", base.getUser());

            var save = pnlUmbraco.Menu.NewButton();
            save.Click += save_Click;
            save.ID = "save";
            save.ToolTip = ui.Text("save");
            save.Text = ui.Text("save");

            pp_alias.Text = umbraco.ui.Text("usertype", base.getUser()) + " " + umbraco.ui.Text("alias", base.getUser());
            pp_name.Text = umbraco.ui.Text("usertype", base.getUser()) + " " + umbraco.ui.Text("name", base.getUser());

            pp_rights.Text = umbraco.ui.Text("default", base.getUser()) + " " + umbraco.ui.Text("rights", base.getUser());
            
            //ensure we have a query string
            if (string.IsNullOrEmpty(Request.QueryString["id"]))
                return;
            //ensuer it is an integer
            if (!int.TryParse(Request.QueryString["id"], out m_userTypeID))
                return;

			if (!IsPostBack)
			{
				BindActions();

				ClientTools
					.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<UserTypes>().Tree.Alias)
					.SyncTree(m_userTypeID.ToString(), false);
			}

        }

        void save_Click(object sender, EventArgs e)
        {
            UserType userType = CurrentUserType;
            userType.Name = txtUserTypeName.Text;
            string actions = "";

            foreach (ListItem li in cbl_rights.Items) {
                if (li.Selected)
                    actions += li.Value;
            }

            userType.DefaultPermissions = actions;
            userType.Save();

            ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "editUserTypeSaved", base.getUser()), "");
        }

        protected List<IAction> CurrentUserTypeActions
        {
            get
            {
                if (m_userTypeActions == null)
                    m_userTypeActions = umbraco.BusinessLogic.Actions.Action.FromString(CurrentUserType.DefaultPermissions);
                return m_userTypeActions;
            }
        }

        protected UserType CurrentUserType
        {
            get
            {
                if (m_userType == null)
                    m_userType = UserType.GetUserType(m_userTypeID);
                return m_userType;
            }
        }
        private UserType m_userType;
        private List<IAction> m_userTypeActions;
        private int m_userTypeID;

        private void BindActions()
        {
            lblUserTypeAlias.Text = CurrentUserType.Alias;
            txtUserTypeName.Text = CurrentUserType.Name;
            hidUserTypeID.Value = CurrentUserType.Id.ToString();

            foreach (IAction ai in global::umbraco.BusinessLogic.Actions.Action.GetPermissionAssignable()) {

                ListItem li = new ListItem(umbraco.ui.Text(ai.Alias, base.getUser()), ai.Letter.ToString());

                if(CurrentUserTypeActions.Contains(ai))
                    li.Selected = true;

                cbl_rights.Items.Add(li);
            }
        }


        
    }
}
