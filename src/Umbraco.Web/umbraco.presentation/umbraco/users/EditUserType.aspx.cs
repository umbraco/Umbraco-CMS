using Umbraco.Core.Services;
using System;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Linq;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.UI;
using Umbraco.Web.UI.Pages;
using Umbraco.Web._Legacy.Actions;
using Action = Umbraco.Web._Legacy.Actions.Action;

namespace umbraco.cms.presentation.user
{
    public partial class EditUserType : UmbracoEnsuredPage
    {
        public EditUserType()
        {
            CurrentApp = Constants.Applications.Users;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            pnlUmbraco.Text = Services.TextService.Localize("usertype");

            var save = pnlUmbraco.Menu.NewButton();
            save.Click += save_Click;
            save.ID = "save";
            save.ToolTip = Services.TextService.Localize("save");
            save.Text = Services.TextService.Localize("save");

            pp_alias.Text = Services.TextService.Localize("usertype") + " " + Services.TextService.Localize("alias");
            pp_name.Text = Services.TextService.Localize("usertype") + " " + Services.TextService.Localize("name");

            pp_rights.Text = Services.TextService.Localize("default") + " " + Services.TextService.Localize("rights");

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
                    .SetActiveTreeType(Constants.Trees.UserTypes)
                    .SyncTree(m_userTypeID.ToString(), false);
            }
        }

        void save_Click(object sender, EventArgs e)
        {
            var userType = CurrentUserType;
            userType.Name = txtUserTypeName.Text;
            string actions = "";

            foreach (ListItem li in cbl_rights.Items) {
                if (li.Selected)
                    actions += li.Value;
            }

            userType.Permissions = actions.ToCharArray().Select(x => x.ToString());
            Services.UserService.SaveUserType(userType);

            ClientTools.ShowSpeechBubble(SpeechBubbleIcon.Save, Services.TextService.Localize("speechBubbles/editUserTypeSaved"), "");
        }

        protected List<IAction> CurrentUserTypeActions
        {
            get
            {
                if (m_userTypeActions == null)
                    m_userTypeActions = Action.FromString(string.Join("", CurrentUserType.Permissions));
                return m_userTypeActions;
            }
        }

        protected IUserType CurrentUserType
        {
            get
            {
                if (m_userType == null)
                    m_userType = Services.UserService.GetUserTypeById(m_userTypeID);
                return m_userType;
            }
        }
        private IUserType m_userType;
        private List<IAction> m_userTypeActions;
        private int m_userTypeID;

        private void BindActions()
        {
            lblUserTypeAlias.Text = CurrentUserType.Alias;
            txtUserTypeName.Text = CurrentUserType.Name;
            hidUserTypeID.Value = CurrentUserType.Id.ToString();

            foreach (IAction ai in Action.GetPermissionAssignable()) {

                ListItem li = new ListItem(Services.TextService.Localize(ai.Alias), ai.Letter.ToString());

                if(CurrentUserTypeActions.Contains(ai))
                    li.Selected = true;

                cbl_rights.Items.Add(li);
            }
        }



    }
}
