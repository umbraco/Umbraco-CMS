using Umbraco.Web.UI;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using Umbraco.Core;

namespace umbraco
{
    public class macroTasks : LegacyDialogTask
    {
        
        
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
            get { return Constants.Applications.Developer.ToString(); }
        }
    }
}
