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
    public class nodetypeTasks : interfaces.ITaskReturnUrl
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
            cms.businesslogic.web.DocumentType dt = cms.businesslogic.web.DocumentType.MakeNew(BusinessLogic.User.GetUser(_userID), Alias.Replace("'", "''"));
            dt.IconUrl = "folder.gif";

            // Create template?
            if (ParentID == 1)
            {
                cms.businesslogic.template.Template[] t = { cms.businesslogic.template.Template.MakeNew(_alias, BusinessLogic.User.GetUser(_userID)) };
                dt.allowedTemplates = t;
                dt.DefaultTemplate = t[0].Id;
            }

            // Master Content Type?
            if (TypeID != 0)
            {
                dt.MasterContentType = TypeID;
            }

            m_returnUrl = "settings/editNodeTypeNew.aspx?id=" + dt.Id.ToString();

            return true;
        }

        public bool Delete()
        {
            new cms.businesslogic.web.DocumentType(ParentID).delete();

            //after a document type is deleted, we clear the cache, as some content will now have disappeared.
            library.RefreshContent();

            return false;
        }

        public nodetypeTasks()
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
