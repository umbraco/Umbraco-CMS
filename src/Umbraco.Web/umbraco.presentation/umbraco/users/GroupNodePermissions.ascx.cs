using System;
using Umbraco.Core.Models.Membership;

namespace umbraco.cms.presentation.user
{
    using Umbraco.Core;

    /// <summary>
    /// An object to display the current permissions for a user group and a node.
    /// </summary>
    public partial class GroupNodePermissions : NodePermissionsBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            DataBind();
        }

        private IUserGroup m_umbracoUserGroup;
        private UserGroupPermissions m_userGroupPermissions;

        public int GroupID
        {
            get { return m_umbracoUserGroup.Id; }
            set
            {
                var userService = ApplicationContext.Current.Services.UserService;
                m_umbracoUserGroup = userService.GetUserGroupById(value);
                m_userGroupPermissions = new UserGroupPermissions(m_umbracoUserGroup);
            }
        }

        public override void DataBind()
        {
            base.DataBind();
            
            if (m_umbracoUserGroup == null)
                throw new ArgumentNullException("No user group specified");

            var selectedNodeId = NodeID[NodeID.Length - 1];
            var groupActions = m_userGroupPermissions.GetExistingNodePermission(selectedNodeId);
            DataBindPermissions(selectedNodeId, groupActions);
        }
   }
}