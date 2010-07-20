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
    public class CreatedPackageTasks : interfaces.ITaskReturnUrl
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

            umbraco.BusinessLogic.User myUser = new umbraco.BusinessLogic.User(0);
            umbraco.BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Delete, myUser, 0, "Xml save started");
            int id = cms.businesslogic.packager.CreatedPackage.MakeNew(Alias).Data.Id;
            m_returnUrl = string.Format("developer/packages/editPackage.aspx?id={0}", id);
            return true;
        }

        public bool Delete()
        {
            // we need to grab the id from the alias as the new tree needs to prefix the NodeID with "package_"
            if (ParentID == 0)
            {
                ParentID = int.Parse(Alias.Substring(loadPackages.PACKAGE_TREE_PREFIX.Length));
            }
            cms.businesslogic.packager.CreatedPackage.GetById(ParentID).Delete();
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
