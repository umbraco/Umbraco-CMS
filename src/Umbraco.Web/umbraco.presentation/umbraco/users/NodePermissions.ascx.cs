using System;
using umbraco.BusinessLogic;

namespace umbraco.cms.presentation.user
{
    /// <summary>
    /// An object to display the current permissions for a user and a node.
    /// </summary>
    public partial class NodePermissions : NodePermissionsBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            DataBind();
        }

        private User m_umbracoUser;
        private UserPermissions m_userPermissions;

        public int UserID
        {
            get { return m_umbracoUser.Id; }
            set
            {
                m_umbracoUser = BusinessLogic.User.GetUser(value);
                m_userPermissions = new UserPermissions(m_umbracoUser);
            }
        }

        public override void DataBind()
        {
            base.DataBind();

            if (m_umbracoUser == null)
                throw new ArgumentNullException("No user group specified");

            var selectedNodeId = NodeID[NodeID.Length - 1];
            var userActions = m_userPermissions.GetExistingNodePermission(selectedNodeId);
            DataBindPermissions(selectedNodeId, userActions);
        }
   }
}