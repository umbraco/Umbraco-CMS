using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using umbraco.interfaces;
using umbraco.BusinessLogic;

namespace umbraco.cms.presentation.user
{
    public class UserTypeTasks : ITaskReturnUrl
    {
        private int m_parentID;
        private int m_typeID;
        private string m_alias;
        private int m_userID;

        #region ITaskReturnUrl Members

        public int ParentID
        {
            get
            {
                return m_parentID;
            }
            set
            {
                m_parentID = value;
            }
        }

        public int TypeID
        {
            get
            {
                return m_typeID;
            }
            set
            {
                m_typeID = value;
            }
        }

        public string Alias
        {
            get
            {
                return m_alias;
            }
            set
            {
                m_alias = value;
            }
        }

        public bool Save()
        {
            try
            {
                UserType u = UserType.MakeNew(this.m_alias, "", this.m_alias);

                m_returnUrl = string.Format("users/EditUserType.aspx?id={0}", u.Id);
                return true;
            }
            catch 
            {
                return false;
            }            
        }

        public bool Delete()
        {
            UserType userType = UserType.GetUserType(this.m_parentID);
            if (userType == null)
                return false;
            userType.Delete();
            return true;
        }

        public int UserId
        {
            set { m_userID = value; }
        }

      
        private string m_returnUrl = "";
        public string ReturnUrl
        {
            get { return m_returnUrl; }
        }

        #endregion
    }
}
