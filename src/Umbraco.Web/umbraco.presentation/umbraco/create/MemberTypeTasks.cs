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
    public class MemberTypeTasks : LegacyDialogTask
    {
       public override bool PerformSave()
        {

            var id = MemberType.MakeNew(User, Alias).Id;
            _returnUrl = string.Format("members/EditMemberType.aspx?id={0}", id);
            return true;
        }

        public override bool PerformDelete()
        {
            new MemberType(ParentID).delete();
            return true;
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
