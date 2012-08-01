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
    public class mediaTasks : interfaces.ITaskReturnUrl
    {

        private string _alias;
        private int _parentID;
        private int _typeID;
        private int _userID;
        private string _returnUrl = "";

        public int UserId
        {
            set { _userID = value; }
        }

        public string ReturnUrl
        {
            get { return _returnUrl; }
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
            cms.businesslogic.media.MediaType dt = new cms.businesslogic.media.MediaType(TypeID);
            cms.businesslogic.media.Media m = cms.businesslogic.media.Media.MakeNew(Alias, dt, BusinessLogic.User.GetUser(_userID), ParentID);
            _returnUrl = "editMedia.aspx?id=" + m.Id.ToString() + "&isNew=true";

            return true;
        }

        public bool Delete()
        {
            cms.businesslogic.media.Media d = new cms.businesslogic.media.Media(ParentID);

            // Log
            BusinessLogic.Log.Add(BusinessLogic.LogTypes.Delete, User.GetCurrent(), d.Id, "");

            d.delete();
            return true;

        }

        public bool Sort()
        {
            return false;
        }

        public mediaTasks()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }
}
