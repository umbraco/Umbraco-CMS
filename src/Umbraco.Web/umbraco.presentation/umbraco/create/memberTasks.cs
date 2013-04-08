using System;
using System.Data;
using System.Web;
using System.Web.Security;
using Umbraco.Web.UI;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using umbraco.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class memberTasks : LegacyDialogTask
    {
        /// <summary>
        /// The new event handler
        /// </summary>
        public delegate void NewUIMemberEventHandler(Member sender, string unencryptedPassword, NewMemberUIEventArgs e);

        public static event NewUIMemberEventHandler NewMember;

        protected virtual void OnNewMember(NewMemberUIEventArgs e, string unencryptedPassword, Member m)
        {
            if (NewMember != null)
            {
                NewMember(m, unencryptedPassword, e);
            }
        }
        
        private int _parentId;        
        private string _returnUrl = "";

        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return DefaultApps.member.ToString(); }
        }

        public override int ParentID
        {
            set
            {
                _parentId = value;
                if (_parentId == 1) _parentId = -1;
            }
            get
            {
                return _parentId;
            }
        }

        public override bool PerformSave()
        {
            var nameAndMail = Alias.Split("|".ToCharArray());
            var name = nameAndMail[0];
            var email = nameAndMail.Length > 0 ? nameAndMail[1] : "";
            var password = nameAndMail.Length > 1 ? nameAndMail[2] : "";
            var loginName = nameAndMail.Length > 2 ? nameAndMail[3] : "";
            if (Member.InUmbracoMemberMode() && TypeID != -1)
            {
                var dt = new MemberType(TypeID);
                var m = Member.MakeNew(name, loginName, email, dt, User);
                m.Password = password;                
                m.LoginName = loginName.Replace(" ", "").ToLower();

                var e = new NewMemberUIEventArgs();
                OnNewMember(e, password, m);

                _returnUrl = "members/editMember.aspx?id=" + m.Id.ToString();
            }
            else
            {
                var mc = new MembershipCreateStatus();
                Membership.CreateUser(name, password, email, "empty", "empty", true, out mc);
                if (mc != MembershipCreateStatus.Success)
                {
                    throw new Exception("Error creating Member: " + mc.ToString());
                }
                _returnUrl = "members/editMember.aspx?id=" + HttpUtility.UrlEncode(name);
            }

            return true;
        }

        public override bool PerformDelete()
        {            
            var u = Membership.GetUser(Alias);
            Membership.DeleteUser(u.UserName, true);
            return true;
        }

    }
}
