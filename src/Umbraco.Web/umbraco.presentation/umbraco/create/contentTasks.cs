using System;
using System.Data;
using System.Web.Security;
using Umbraco.Web.UI;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using umbraco.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class contentTasks : LegacyDialogTask
    {
        private string _returnUrl = "";

        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return DefaultApps.content.ToString(); }
        }

        public override bool PerformSave()
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

        public override bool PerformDelete()
        {
            var d = new cms.businesslogic.web.Document(ParentID);

            d.delete();

            // Log
            Log.Add(LogTypes.Delete, User.GetCurrent(), d.Id, "");

            return true;

        }

    }
}
