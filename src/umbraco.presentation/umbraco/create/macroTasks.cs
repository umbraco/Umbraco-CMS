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
    public class macroTasks : interfaces.ITaskReturnUrl
    {

        private string _alias;
        private int _parentID;
        private int _typeID;
        private int _userID;

        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

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
            int id = umbraco.cms.businesslogic.macro.Macro.MakeNew(_alias).Id;
            m_returnUrl = string.Format("developer/Macros/editMacro.aspx?macroID={0}", id);
            return true;
        }

        public bool Delete()
        {
            // Clear cache!
            macro.GetMacro(ParentID).removeFromCache();
            new cms.businesslogic.macro.Macro(ParentID).Delete();
            return true;
        }

        public macroTasks()
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
