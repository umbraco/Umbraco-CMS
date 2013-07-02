using System;
using System.Data;
using System.Web.Security;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using umbraco.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class dictionaryTasks : interfaces.ITaskReturnUrl
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
            set
            {
                _parentID = value;
                // NASTY HACK ON NASTY HACK§!!
                // if (_parentID == 1) _parentID = -1;
            }
            get
            {
                return _parentID;
            }
        }

        public bool Save()
        {
            //check to see if key is already there
            if (cms.businesslogic.Dictionary.DictionaryItem.hasKey(Alias))
                return false;

            // Create new dictionary item if name no already exist
            if (ParentID > 0)
            {
                // Prepend parent keys to keep unique
                var parentKey = new cms.businesslogic.Dictionary.DictionaryItem(ParentID).key;

                int id = cms.businesslogic.Dictionary.DictionaryItem.addKey(parentKey + "." + Alias, "", parentKey);
                m_returnUrl = string.Format("settings/editDictionaryItem.aspx?id={0}", id);
            }
            else
            {
                int id = cms.businesslogic.Dictionary.DictionaryItem.addKey(Alias, "");
                m_returnUrl = string.Format("settings/editDictionaryItem.aspx?id={0}", id);
            }
            return true;
        }

        public bool Delete()
        {
			LogHelper.Debug<dictionaryTasks>(_typeID.ToString() + " " + _parentID.ToString() + " deleting " + Alias);

            new cms.businesslogic.Dictionary.DictionaryItem(ParentID).delete();
            return true;
        }

        public bool Sort()
        {
            return false;
        }

        public dictionaryTasks()
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
