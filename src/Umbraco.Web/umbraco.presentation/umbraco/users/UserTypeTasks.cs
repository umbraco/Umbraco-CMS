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
using Umbraco.Core;
using Umbraco.Core.Models.Membership;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web._Legacy.UI;

namespace umbraco.cms.presentation.user
{
    public class UserTypeTasks : LegacyDialogTask
    {
        public override bool PerformSave()
        {
            try
            {
                var u = new UserType(Alias, Alias);
                Current.Services.UserService.SaveUserType(u);
                _returnUrl = string.Format("users/EditUserType.aspx?id={0}", u.Id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public override bool PerformDelete()
        {
            var userType = Current.Services.UserService.GetUserTypeById(ParentID);
            if (userType == null)
                return false;
            Current.Services.UserService.DeleteUserType(userType);
            return true;
        }

        private string _returnUrl = "";
        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return Constants.Applications.Users.ToString(); }
        }
    }
}
