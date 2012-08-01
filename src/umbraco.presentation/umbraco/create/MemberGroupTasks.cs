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
    public class MemberGroupTasks : interfaces.ITaskReturnUrl
    {

        private string _alias;
        private int _parentID;
        private int _typeID;
        private int _userID;

        public int UserId
        {
            set { _userID = value; }
        }
        public int TypeID
        {
            set { _typeID = value; }
            get { return _typeID; }
        }


        public string Alias
        {
            set { _alias = value; }
            get { return _alias; }
        }

        public int ParentID
        {
            set { _parentID = value; }
            get { return _parentID; }
        }

        public bool Save()
        {
            System.Web.Security.Roles.CreateRole(Alias);
            //			int id = cms.businesslogic.member.MemberGroup.MakeNew(Alias, BusinessLogic.User.GetUser(_userID)).Id;
            m_returnUrl = string.Format("members/EditMemberGroup.aspx?id={0}", System.Web.HttpContext.Current.Server.UrlEncode(Alias));
            return true;
        }

        public bool Delete()
        {
            // only build-in roles can be deleted
            if (cms.businesslogic.member.Member.IsUsingUmbracoRoles())
            {
                cms.businesslogic.member.MemberGroup.GetByName(Alias).delete();
                return true;
            }
            else
            {
                return false;
            }
            //try
            //{

            //    MembershipUser u = Membership.GetUser(_parentID);
            //    Membership.DeleteUser(u.UserName);
            //    return true;

            //}catch
            //{
            //    Log.Add(LogTypes.Error, _parentID, "Member cannot be deleted.");
            //    return false;
            //}
        }

        public MemberGroupTasks()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #region ITaskReturnUrl Members
        private string m_returnUrl = "";
        public string ReturnUrl
        {
            get { return m_returnUrl; }
        }

        #endregion
    }
}
