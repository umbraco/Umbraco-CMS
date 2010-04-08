using System;
using System.Data;
using System.Web.Security;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using umbraco.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class memberTasks : interfaces.ITaskReturnUrl
    {
        /// <summary>
        /// The new event handler
        /// </summary>
        new public delegate void NewUIMemberEventHandler(Member sender, string unencryptedPassword, NewMemberUIEventArgs e);

        new public static event NewUIMemberEventHandler NewMember;
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
            if (cms.businesslogic.member.Member.InUmbracoMemberMode() && TypeID != -1)
            {
                cms.businesslogic.member.MemberType dt = new cms.businesslogic.member.MemberType(TypeID);
                cms.businesslogic.member.Member m = cms.businesslogic.member.Member.MakeNew(name, dt, BusinessLogic.User.GetUser(_userID));
                m.Password = password;
                m.Email = email;
                m.LoginName = name.Replace(" ", "").ToLower();

                NewMemberUIEventArgs e = new NewMemberUIEventArgs();
                this.OnNewMember(e, password, m);

                _returnUrl = "members/editMember.aspx?id=" + m.Id.ToString();
            }
            else
            {
                MembershipCreateStatus mc = new MembershipCreateStatus();
                Membership.CreateUser(name, password, email, "empty", "empty", true, out mc);
                if (mc != MembershipCreateStatus.Success)
                {
                    throw new Exception("Error creating Member: " + mc.ToString());
                }
                _returnUrl = "members/editMember.aspx?id=" + System.Web.HttpContext.Current.Server.UrlEncode(name);
            }

            return true;
        }

        public bool Delete()
        {
            //cms.businesslogic.member.Member d = new cms.businesslogic.member.Member(ParentID);
            //d.delete();
            //return true;
            MembershipUser u = Membership.GetUser(Alias);
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
