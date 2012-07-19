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
    public class contentItemTasks : interfaces.ITask
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
            }
            get
            {
                return _parentID;
            }
        }

        public bool Save()
        {
            // TODO : fix it!!
            return true;
        }

        public bool Delete()
        {
            cms.businesslogic.contentitem.ContentItem d = new cms.businesslogic.contentitem.ContentItem(ParentID);

            // Version3.0 - moving to recycle bin instead of deletion
            //d.delete();
            d.Move(-20);
            return true;

        }

        public bool Sort()
        {
            return false;
        }

        public contentItemTasks()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }
}
