using System;
using System.Data;
using System.Web.Security;
using Umbraco.Web.UI;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class MemberGroupTasks : LegacyDialogTask
    {
        public override bool PerformSave()
        {
            Roles.CreateRole(Alias);
            _returnUrl = string.Format("members/EditMemberGroup.aspx?id={0}", System.Web.HttpContext.Current.Server.UrlEncode(Alias));
            return true;
        }

        public override bool PerformDelete()
        {
            // only built-in roles can be deleted
            if (Member.IsUsingUmbracoRoles())
            {
                MemberGroup.GetByName(Alias).delete();
                return true;
            }
            return false;            
        }

        private string _returnUrl = "";

        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return DefaultApps.member.ToString(); }
        }
    }
}
