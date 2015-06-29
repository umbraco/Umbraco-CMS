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
    public class stylesheetPropertyTasks : LegacyDialogTask
    {

        public override bool PerformSave()
        {
            try
            {
                var s = new cms.businesslogic.web.StyleSheet(ParentID);
                var id = s.AddProperty(Alias, User).Id;
                _returnUrl = string.Format("settings/stylesheet/property/EditStyleSheetProperty.aspx?id={0}", id);
            }
            catch
            {
                throw new ArgumentException("DER ER SKET EN FEJL MED AT OPRETTE NOGET MED ET PARENT ID : " + ParentID);
            }
            return true;
        }

        public override bool PerformDelete()
        {
            var sp = new cms.businesslogic.web.StylesheetProperty(ParentID);
            var s = sp.StyleSheet();
            s.saveCssToFile();
            sp.delete();

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
