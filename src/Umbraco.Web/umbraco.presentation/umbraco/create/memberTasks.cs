using System;
using System.Data;
using System.Web;
using System.Globalization;
using System.Web.Security;
using Umbraco.Web.UI;
using Umbraco.Core.Security;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    [Obsolete("this is no longer used and will be removed from the codebase in the future")]
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

            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();

            if (provider.IsUmbracoMembershipProvider() && TypeID != -1)
            {
                var dt = new MemberType(TypeID);
                var castedProvider = (UmbracoMembershipProviderBase)provider;
                MembershipCreateStatus status;
                
                //First create with the membership provider
                //TODO: We are not supporting q/a - passing in empty here
                var created = castedProvider.CreateUser(dt.Alias, 
                    loginName.Replace(" ", "").ToLower(), //dunno why we're doing this but that's how it has been so i'll leave it i guess
                    password, email, "", "", true, Guid.NewGuid(), out status);
                if (status != MembershipCreateStatus.Success)
                {
                    throw new Exception("Error creating Member: " + status);
                }

                //update the name
                var member = Member.GetMemberFromLoginName(created.UserName);
                member.Text = name;
                member.Save();

                var e = new NewMemberUIEventArgs();
                this.OnNewMember(e, password, member);

                _returnUrl = "members/editMember.aspx?id=" + member.Id.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                MembershipCreateStatus mc;
                provider.CreateUser(name, password, email, "empty", "empty", true, null, out mc);
                if (mc != MembershipCreateStatus.Success)
                {
                    throw new Exception("Error creating Member: " + mc);
                }
                _returnUrl = "members/editMember.aspx?id=" + HttpUtility.UrlEncode(name);
            }

            return true;
        }

        public override bool PerformDelete()
        {            
            var provider = MembershipProviderExtensions.GetMembersMembershipProvider();
            var u = provider.GetUser(Alias, false);
            if (u == null) return false;
            provider.DeleteUser(u.UserName, true);
            return true;


        }

    }
}
