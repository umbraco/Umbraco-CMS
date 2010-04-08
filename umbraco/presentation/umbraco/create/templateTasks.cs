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
    public class templateTasks : interfaces.ITaskReturnUrl
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
            int masterId = ParentID;
            BusinessLogic.Log.Add(LogTypes.Debug, -1, "tp id:" + masterId.ToString());
            if (masterId > 0)
            {
                int id = cms.businesslogic.template.Template.MakeNew(Alias, BusinessLogic.User.GetUser(_userID), new cms.businesslogic.template.Template(masterId)).Id;
                m_returnUrl = string.Format("settings/editTemplate.aspx?templateID={0}", id);
            }
            else
            {
                int id = cms.businesslogic.template.Template.MakeNew(Alias, BusinessLogic.User.GetUser(_userID)).Id;
                m_returnUrl = string.Format("settings/editTemplate.aspx?templateID={0}", id);

            }
            return true;
        }

        public bool Delete()
        {
            new cms.businesslogic.template.Template(_parentID).delete();
            return false;
        }

        public templateTasks()
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
