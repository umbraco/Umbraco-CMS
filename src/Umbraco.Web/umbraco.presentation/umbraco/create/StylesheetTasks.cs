using System;
using System.Data;
using System.Web.Security;
using Umbraco.Web.UI;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class StylesheetTasks : LegacyDialogTask
    {
        public override bool PerformSave()
        {

            int id = -1;
            var checkingSheet = cms.businesslogic.web.StyleSheet.GetByName(Alias) ?? cms.businesslogic.web.StyleSheet.MakeNew(User, Alias, "", "");
            _returnUrl = string.Format("settings/stylesheet/editStylesheet.aspx?id={0}", checkingSheet.Text);
            return true;
        }

        public override bool PerformDelete()
        {
            var s = cms.businesslogic.web.StyleSheet.GetByName(Alias);
            s.delete();
            return true;
        }

        private string _returnUrl = "";
        
        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return DefaultApps.settings.ToString(); }
        }
    }
}
