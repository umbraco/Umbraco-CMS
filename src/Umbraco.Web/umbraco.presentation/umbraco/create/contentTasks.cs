using System;
using System.Data;
using System.Web.Security;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class contentTasks : interfaces.ITaskReturnUrl
    {

        private string _alias;
        private int _parentId;
        private int _typeId;
        private int _userId;
        private string _returnUrl = "";

        public int UserId
        {
            set { _userId = value; }
        }

        public string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public int TypeID
        {
            set { _typeId = value; }
            get { return _typeId; }
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
                _parentId = value;
            }
            get
            {
                return _parentId;
            }
        }

        public bool Save()
        {
            var dt = new cms.businesslogic.web.DocumentType(TypeID);
            var d = cms.businesslogic.web.Document.MakeNew(Alias, dt, User.GetUser(_userId), ParentID);
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
            var d = new cms.businesslogic.web.Document(ParentID);

            d.delete();

            // Log
            Log.Add(LogTypes.Delete, User.GetCurrent(), d.Id, "");

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
