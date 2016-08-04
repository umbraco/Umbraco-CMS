using System;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;
using Umbraco.Core.Models.Membership;

namespace umbraco.cms.presentation.user
{
    public partial class GroupPermissionEditor : PermissionEditorBase
    {
        #region Controls

        protected global::umbraco.cms.presentation.user.GroupNodePermissions nodePermissions;

        #endregion

        public override string PersmissionsHandlerServiceName
        {
            get { return "GroupPermissionsHandler"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Request.QueryString["id"]))
                return;

            CheckEntity(Request.QueryString["id"]);

            nodePermissions.GroupID = Convert.ToInt32(Request.QueryString["id"]);
            pnlUmbraco.Text = ui.Text("user", "userGroupPermissions");

            InitControls();
            InitTree(TreeDefinitionCollection.Instance.FindTree<Trees.UserGroupPermissions>());
        }

        /// <summary>
        /// Makes sure the group exists with the id specified
        /// </summary>
        /// <param name="strID"></param>
        protected override void CheckEntity(string strID)
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
        }
    }
}
