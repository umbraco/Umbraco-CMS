using System;
using System.Linq;
using Umbraco.Core.Models.Membership;
using System.Collections.Generic;
using umbraco.BusinessLogic.Actions;
using umbraco.interfaces;
using Umbraco.Core;

namespace umbraco.cms.presentation.user
{
    using Umbraco.Web;

    /// <summary>
    /// An object to display the current permissions for a user group and a node.
    /// </summary>
    public partial class NodePermissions : System.Web.UI.UserControl
    {
        private IUserGroup m_umbracoUserGroup;
        private UserGroupPermissions m_userGroupPermissions;
        private int[] m_nodeID = { -1 };
        private string m_clientItemChecked = "void(0);";
        private bool m_viewOnly = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            DataBind();
        }

        public int UserGroupID
        {
            get { return m_umbracoUserGroup.Id; }
            set
            {
                var userService = ApplicationContext.Current.Services.UserService;
                m_umbracoUserGroup = userService.GetUserGroupById(value);
                m_userGroupPermissions = new UserGroupPermissions(m_umbracoUserGroup);
            }
        }

        public bool ViewOnly
        {
            get { return m_viewOnly; }
            set { m_viewOnly = value; }
        }

        public int[] NodeID
        {
            get { return m_nodeID; }
            set { m_nodeID = value; }
        }

        /// <summary>
        /// The JavaScript method to call when a node is checked. The method will receive a comma separated list of node IDs that are checked.
        /// </summary>
        public string OnClientItemChecked
        {
            get { return m_clientItemChecked; }
            set { m_clientItemChecked = value; }
        }

        public override void DataBind()
        {
            base.DataBind();

            if (m_umbracoUserGroup == null)
                throw new ArgumentNullException("No user group specified");

            //lookup permissions for last node selected
            var selectedNodeId = m_nodeID[m_nodeID.Length - 1];

            //get the logged in user's permissions
            var userService = ApplicationContext.Current.Services.UserService;
            var currUserPermissions = userService.GetPermissions(UmbracoContext.Current.Security.CurrentUser, selectedNodeId).Single();

            var lstCurrUserActions = BusinessLogic.Actions.Action.FromString(string.Join(string.Empty, currUserPermissions.AssignedPermissions));
            var lstLookupGroupActions = m_userGroupPermissions.GetExistingNodePermission(selectedNodeId);

            var lstAllActions = BusinessLogic.Actions.Action.GetPermissionAssignable();

            //no node is selected, disable the check boxes
            if (NodeID[0] == -1)
            {
                ShowMessage("No node selected");
                return;
            }

            //ensure the current user has access to assign permissions.
            if (lstCurrUserActions.Contains(ActionRights.Instance))
                BindExistingPermissions(lstAllActions, lstLookupGroupActions);
            else
                ShowMessage("You do not have access to assign permissions to this node");

            var names = string.Empty;
            foreach (int id in NodeID)
            {
                if (id > 0)
                {
                    names += new cms.businesslogic.web.Document(id).Text + ", ";
                }
            }

            lt_names.Text = names.Trim().Trim(',');
        }

        protected void ShowMessage(string msg)
        {
            lblMessage.Visible = true;
            lblMessage.Text = msg;
        }

        protected void BindExistingPermissions(List<IAction> allActions, List<IAction> userActions)
        {
            List<AssignedPermission> assignedPermissions = new List<AssignedPermission>();
            foreach (umbraco.interfaces.IAction a in allActions)
            {
                AssignedPermission p = new AssignedPermission();
                p.Permission = a;
                p.HasPermission = (userActions != null ? userActions.Contains(a) : false);
                assignedPermissions.Add(p);
            }

            rptPermissionsList.DataSource = assignedPermissions;
            rptPermissionsList.DataBind();
        }

        protected struct AssignedPermission
        {
            public IAction Permission;
            public bool HasPermission;
        }
    }
}