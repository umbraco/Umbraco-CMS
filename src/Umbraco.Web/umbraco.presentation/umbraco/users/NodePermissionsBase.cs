using System.Collections.Generic;
using umbraco.BasePages;
using umbraco.BusinessLogic.Actions;
using umbraco.interfaces;

namespace umbraco.cms.presentation.user
{
    /// <summary>
    /// Provides base functionality for controls displaying the current permissions for a user or group and a node.
    /// </summary>
    public partial class NodePermissionsBase : System.Web.UI.UserControl
    {
        #region Controls

        protected global::System.Web.UI.WebControls.Literal lt_names;

        protected global::System.Web.UI.WebControls.Panel pnlReplaceChildren;

        protected global::System.Web.UI.WebControls.Label lblMessage;

        protected global::System.Web.UI.WebControls.Repeater rptPermissionsList;

        #endregion

        private int[] m_nodeID = { -1 };
        private string m_clientItemChecked = "void(0);";

        private bool m_viewOnly = false;
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

        protected void DataBindPermissions(int selectedNodeId, List<IAction> lstUserOrGroupActions)
        {
            //get the logged in user's permissions
            var currUserPermissions = new UserPermissions(UmbracoEnsuredPage.CurrentUser);

            var lstCurrUserActions = currUserPermissions.GetExistingNodePermission(selectedNodeId);
            var lstAllActions = umbraco.BusinessLogic.Actions.Action.GetPermissionAssignable();

            //no node is selected, disable the check boxes
            if (NodeID[0] == -1)
            {
                ShowMessage("No node selected");
                return;
            }

            //ensure the current user has access to assign permissions.
            //if their actions list is null then it means that the node is not published.
            if (lstCurrUserActions == null || lstCurrUserActions.Contains(ActionRights.Instance))
                BindExistingPermissions(lstAllActions, lstUserOrGroupActions);
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