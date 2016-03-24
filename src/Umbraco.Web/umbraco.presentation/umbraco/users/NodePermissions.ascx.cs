using System;
using System.Collections.Generic;
using umbraco.BusinessLogic;
using Umbraco.Core.Models.Membership;
using Umbraco.Web;
using Umbraco.Web.UI.Controls;
using Umbraco.Web._Legacy.Actions;
using Action = Umbraco.Web._Legacy.Actions.Action;

namespace umbraco.cms.presentation.user
{

    /// <summary>
    /// An object to display the current permissions for a user and a node.
    /// </summary>
    public partial class NodePermissions : UmbracoUserControl
    {

        protected override void OnInit(EventArgs e) {
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            DataBind();
        }

        private IUser m_umbracoUser;
        private int[] m_nodeID = {-1};
        private UserPermissions m_userPermissions;
        private string m_clientItemChecked = "void(0);";

        public int UserID
        {
            get { return m_umbracoUser.Id; }
            set
            {
                m_umbracoUser = Services.UserService.GetUserById(value);
                m_userPermissions = new UserPermissions(m_umbracoUser);
            }
        }

        private bool m_viewOnly = false;
        public bool ViewOnly {
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
            
            
            if (m_umbracoUser == null)
                throw new ArgumentNullException("No User specified");

            //get the logged in user's permissions
            UserPermissions currUserPermissions = new UserPermissions(Security.CurrentUser);
            
            //lookup permissions for last node selected
            int selectedNodeId = m_nodeID[m_nodeID.Length - 1];
            
            List<IAction> lstCurrUserActions = currUserPermissions.GetExistingNodePermission(selectedNodeId);
            List<IAction> lstLookupUserActions = m_userPermissions.GetExistingNodePermission(selectedNodeId);
            
            List<IAction> lstAllActions = Action.GetPermissionAssignable();

            //no node is selected, disable the check boxes
            if (m_nodeID[0] == -1)
            {
                ShowMessage("No node selected");
                return;
            }

            //ensure the current user has access to assign permissions.
            //if their actions list is null then it means that the node is not published.
            if (lstCurrUserActions == null || lstCurrUserActions.Contains(ActionRights.Instance))
                BindExistingPermissions(lstAllActions, lstLookupUserActions);
            else
                ShowMessage("You do not have access to assign permissions to this node");

            string names = "";
            foreach (int id in m_nodeID) {
                if(id > 0)
                    names += new cms.businesslogic.web.Document(id).Text + ", ";
            }

			lt_names.Text = names.Trim().Trim(',');
        }

        private void ShowMessage(string msg)
        {
            lblMessage.Visible = true;
            lblMessage.Text = msg;
            
        }

        private void BindExistingPermissions(List<IAction> allActions, List<IAction> userActions)
        {
            
            List<AssignedPermission> assignedPermissions = new List<AssignedPermission>();
            foreach (var a in allActions)
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