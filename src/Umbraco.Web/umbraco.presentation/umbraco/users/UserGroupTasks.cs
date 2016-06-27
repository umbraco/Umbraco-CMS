using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Umbraco.Web.UI;
using umbraco.interfaces;
using umbraco.BusinessLogic;

namespace umbraco.cms.presentation.user
{
    public class UserGroupTasks : LegacyDialogTask
    {
        public override bool PerformSave()
        {
            try
            {
                var u = UserGroup.MakeNew(Alias, "", Alias);
                _returnUrl = string.Format("users/EditUserGroup.aspx?id={0}", u.Id);
                return true;
            }
            catch 
            {
                return false;
            }            
        }

        public override bool PerformDelete()
        {
            var userGroup = UserGroup.GetUserGroup(ParentID);
            if (userGroup == null)
                return false;
            userGroup.Delete();
            return true;
        }

        private string _returnUrl = "";
        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return DefaultApps.users.ToString(); }
        }
    }
}
