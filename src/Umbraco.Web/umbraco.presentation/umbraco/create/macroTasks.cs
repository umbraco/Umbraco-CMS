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
    public class macroTasks : LegacyDialogTask
    {
        
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }
        
        public override bool PerformSave()
        {
            var checkingMacro =cms.businesslogic.macro.Macro.GetByAlias(Alias);
            var id = checkingMacro != null
                         ? checkingMacro.Id
                         : cms.businesslogic.macro.Macro.MakeNew(Alias).Id;
            _returnUrl = string.Format("developer/Macros/editMacro.aspx?macroID={0}", id);
            return true;
        }

        public override bool PerformDelete()
        {
            new cms.businesslogic.macro.Macro(ParentID).Delete();
            return true;
        }

        private string _returnUrl = "";

        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return DefaultApps.developer.ToString(); }
        }
    }
}
