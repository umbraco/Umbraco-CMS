using System;
using umbraco.cms.presentation.Trees;

namespace umbraco.cms.presentation.user
{
    public partial class PermissionEditor : PermissionEditorBase
    {
        #region Controls

        protected global::umbraco.cms.presentation.user.NodePermissions nodePermissions;

        #endregion

        public override string PersmissionsHandlerServiceName
        {
            get { return "PermissionsHandler"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Request.QueryString["id"]))
                return;

            CheckEntity(Request.QueryString["id"]);

            nodePermissions.UserID = Convert.ToInt32(Request.QueryString["id"]);
            pnlUmbraco.Text = ui.Text("user", "userGroupPermissions");

            InitControls();
            InitTree(TreeDefinitionCollection.Instance.FindTree<Trees.UserPermissions>());
        }

        /// <summary>
        /// Makes sure the user exists with the id specified
        /// </summary>
        /// <param name="strID"></param>
        protected override void CheckEntity(string strID)
        {
            int id;
            bool parsed = false;
            umbraco.BusinessLogic.User oUser = null;
            if (parsed = int.TryParse(strID, out id))
            {
                oUser = umbraco.BusinessLogic.User.GetUser(id);
            }

            if (oUser == null || oUser.UserType == null || parsed == false)
            {
                throw new Exception("No user found with id: " + strID);
            }
        }
    }
}
