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
    public class contentTasks : interfaces.ITaskReturnUrl
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
            cms.businesslogic.web.DocumentType dt = new cms.businesslogic.web.DocumentType(TypeID);
            cms.businesslogic.web.Document d = cms.businesslogic.web.Document.MakeNew(Alias, dt, BusinessLogic.User.GetUser(_userID), ParentID);
            if (d == null)
            {
                //TODO: Slace - Fix this to use the language files
                BasePage.Current.ClientTools.ShowSpeechBubble(BasePage.speechBubbleIcon.error, "Document Creation", "Document creation was canceled");
                return false;
            }
            else
            {
                _returnUrl = "editContent.aspx?id=" + d.Id.ToString() + "&isNew=true";
                return true;
            }
        }

        public bool Delete()
        {
            cms.businesslogic.web.Document d = new cms.businesslogic.web.Document(ParentID);

            // Log
            BusinessLogic.Log.Add(BusinessLogic.LogTypes.Delete, User.GetCurrent(), d.Id, "");

            library.UnPublishSingleNode(d.Id);

            d.delete();

            return true;

        }

        public bool Sort()
        {
            return false;
        }

        public contentTasks()
        {
            //
            // TODO: Add constructor logic here
            //
        }
    }
}
