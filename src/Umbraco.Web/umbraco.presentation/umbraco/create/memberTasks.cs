using System;
using System.Data;
using System.Globalization;
using System.Web.Security;
using Umbraco.Core.Security;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class memberTasks : interfaces.ITaskReturnUrl
    {
        /// <summary>
        /// The new event handler
        /// </summary>
        public delegate void NewUIMemberEventHandler(Member sender, string unencryptedPassword, NewMemberUIEventArgs e);

        public static event NewUIMemberEventHandler NewMember;
        new protected virtual void OnNewMember(NewMemberUIEventArgs e, string unencryptedPassword, Member m)
        {
            if (NewMember != null)
            {
                NewMember(m, unencryptedPassword, e);
            }
        }


        private string _alias;
        private int _parentID;
        private int _typeID;
        private int _userID;
        private string _returnUrl = "";

        public int UserId
        {
            set { _userID = value; }
        }

        public int TypeID
        {
            set { _typeID = value; }
            get { return _typeID; }
        }

        public string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public string Alias
        {
            set { _alias = value; }
            get { return _alias; }
        }

        public int ParentID
        {
            set
            {
                _parentID = value;
                if (_parentID == 1) _parentID = -1;
            }
            get
            {
                return _parentID;
            }
        }

        public bool Save()
        {
            string[] nameAndMail = Alias.Split("|".ToCharArray());
            string name = nameAndMail[0];
            string email = nameAndMail.Length > 0 ? nameAndMail[1] : "";
            string password = nameAndMail.Length > 1 ? nameAndMail[2] : "";
            string loginName = nameAndMail.Length > 2 ? nameAndMail[3] : "";
            if (Membership.Provider.IsUmbracoMembershipProvider() && TypeID != -1)
            {
                var dt = new MemberType(TypeID);                
                var provider = (providers.members.UmbracoMembershipProvider) Membership.Provider;
                MembershipCreateStatus status;
                //TODO: We are not supporting q/a - passing in empty here
                var created = provider.CreateUser(dt.Alias, 
                    loginName.Replace(" ", "").ToLower(), //dunno why we're doing this but that's how it has been so i'll leave it i guess
                    password, email, "", "", true, Guid.NewGuid(), out status);

                var member = Member.GetMemberFromLoginName(created.UserName);

                var e = new NewMemberUIEventArgs();
                this.OnNewMember(e, password, member);

                _returnUrl = "members/editMember.aspx?id=" + member.Id.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                var mc = new MembershipCreateStatus();
                Membership.CreateUser(name, password, email, "empty", "empty", true, out mc);
                if (mc != MembershipCreateStatus.Success)
                {
                    throw new Exception("Error creating Member: " + mc);
                }
                _returnUrl = "members/editMember.aspx?id=" + System.Web.HttpContext.Current.Server.UrlEncode(name);
            }

            return true;
        }

        public bool Delete()
        {
            var u = Membership.GetUser(Alias);
            if (u == null) return false;
            Membership.DeleteUser(u.UserName, true);
            return true;


        }

        public bool Sort()
        {
            return false;
        }

        public memberTasks()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }
}
