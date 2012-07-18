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
    public class stylesheetPropertyTasks : interfaces.ITaskReturnUrl
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
            try
            {
                cms.businesslogic.web.StyleSheet s = new cms.businesslogic.web.StyleSheet(ParentID);
                int id = s.AddProperty(Alias, BusinessLogic.User.GetUser(_userID)).Id;
                m_returnUrl = string.Format("settings/stylesheet/property/EditStyleSheetProperty.aspx?id={0}", id);
            }
            catch
            {
                throw new ArgumentException("DER ER SKET EN FEJL MED AT OPRETTE NOGET MED ET PARENT ID : " + ParentID);
            }
            return true;
        }

        public bool Delete()
        {
            cms.businesslogic.web.StylesheetProperty sp = new cms.businesslogic.web.StylesheetProperty(ParentID);
            cms.businesslogic.web.StyleSheet s = sp.StyleSheet();
            s.saveCssToFile();
            sp.delete();

            return true;
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
