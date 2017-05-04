using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.presentation.Trees;
using umbraco.interfaces;
using Umbraco.Core;

namespace umbraco.cms.presentation.user
{
    public partial class EditUserGroup : EditUserGroupsBase
    {
        public EditUserGroup()
        {
            CurrentApp = BusinessLogic.DefaultApps.users.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            pnlUmbraco.Text = umbraco.ui.Text("usergroup", base.getUser());

            var save = pnlUmbraco.Menu.NewButton();
            save.Click += save_Click;
            save.ID = "save";
            save.ToolTip = ui.Text("save");
            save.Text = ui.Text("save");

            pp_alias.Text = umbraco.ui.Text("alias", base.getUser());
            pp_name.Text = umbraco.ui.Text("name", base.getUser());

            pp_rights.Text = umbraco.ui.Text("default", base.getUser()) + " " + umbraco.ui.Text("rights", base.getUser());
            pp_sections.Text = umbraco.ui.Text("sections", base.getUser());

            pp_users.Text = umbraco.ui.Text("users", base.getUser());

            //ensure we have a query string
            if (string.IsNullOrEmpty(Request.QueryString["id"]))
                return;
            //ensure it is an integer
            if (!int.TryParse(Request.QueryString["id"], out m_userGroupID))
                return;

            if (!IsPostBack)
            {
                BindDetails();
                BindActions();
                BindSections();
                BindUsers();
                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<UserGroups>().Tree.Alias)
                    .SyncTree(m_userGroupID.ToString(), false);
            }
        }

        void save_Click(object sender, EventArgs e)
        {
            var userGroup = CurrentUserGroup;
            userGroup.Name = txtUserGroupName.Text;
            string actions = "";

            foreach (ListItem li in cbl_rights.Items)
            {
                if (li.Selected)
                    actions += li.Value;
            }

            userGroup.DefaultPermissions = actions;

            var userIds = new List<int>();
            foreach (ListItem li in lstUsersInGroup.Items)
            {
                userIds.Add(int.Parse(li.Value));
            }

            userGroup.ClearApplications();
            foreach (ListItem li in cbl_sections.Items)
            {
                if (li.Selected) userGroup.AddApplication(li.Value);
            }

            userGroup.SaveWithUsers(userIds.ToArray());

            ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "editUserGroupSaved", base.getUser()), "");
        }

        protected List<IAction> CurrentUserGroupActions
        {
            get
            {
                if (m_userGroupActions == null)
                    m_userGroupActions = umbraco.BusinessLogic.Actions.Action.FromString(CurrentUserGroup.DefaultPermissions);
                return m_userGroupActions;
            }
        }

        protected UserGroup CurrentUserGroup
        {
            get
            {
                if (m_userGroup == null)
                    m_userGroup = UserGroup.GetUserGroup(m_userGroupID);
                return m_userGroup;
            }
        }

        private UserGroup m_userGroup;
        private List<IAction> m_userGroupActions;
        private int m_userGroupID;

        private void BindDetails()
        {
            lblUserGroupAlias.Text = CurrentUserGroup.Alias;
            txtUserGroupName.Text = CurrentUserGroup.Name;
            hidUserGroupID.Value = CurrentUserGroup.Id.ToString();
        }

        private void BindActions()
        {
            foreach (IAction ai in global::umbraco.BusinessLogic.Actions.Action.GetPermissionAssignable())
            {

                ListItem li = new ListItem(umbraco.ui.Text(ai.Alias, base.getUser()), ai.Letter.ToString());

                if (CurrentUserGroupActions.Contains(ai))
                    li.Selected = true;

                cbl_rights.Items.Add(li);
            }
        }

        private void BindSections()
        {
            string currentUserApps = ";";
            foreach (Application a in CurrentUser.Applications)
                currentUserApps += a.alias + ";";

            Application[] gapps = CurrentUserGroup.Applications;
            foreach (Application app in BusinessLogic.Application.getAll())
            {
                if (CurrentUser.IsAdmin() || currentUserApps.Contains(";" + app.alias + ";"))
                {
                    ListItem li = new ListItem(ui.Text("sections", app.alias), app.alias);
                    foreach (Application tmp in gapps) if (app.alias == tmp.alias) li.Selected = true;
                    cbl_sections.Items.Add(li);
                }
            }
        }

        private void BindUsers()
        {
            var userService = ApplicationContext.Current.Services.UserService;

            lstUsersInGroup.DataSource = userService.GetAllInGroup(CurrentUserGroup.Id);
            lstUsersInGroup.DataValueField = "Id";
            lstUsersInGroup.DataTextField = "Name";
            lstUsersInGroup.DataBind();

            lstUsersNotInGroup.DataSource = userService.GetAllNotInGroup(CurrentUserGroup.Id);
            lstUsersNotInGroup.DataValueField = "Id";
            lstUsersNotInGroup.DataTextField = "Name";
            lstUsersNotInGroup.DataBind();
        }

        protected void btnAddToGroup_Click(object sender, EventArgs e)
        {
            MoveItems(lstUsersNotInGroup, lstUsersInGroup);
        }

        protected void btnRemoveFromGroup_Click(object sender, EventArgs e)
        {
            MoveItems(lstUsersInGroup, lstUsersNotInGroup);
        }
    }
}