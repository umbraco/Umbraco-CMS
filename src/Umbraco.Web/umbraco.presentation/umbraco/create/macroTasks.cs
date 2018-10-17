using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Composing;
using Umbraco.Web._Legacy.UI;

namespace Umbraco.Web
{
    public class macroTasks : LegacyDialogTask
    {
        public override bool PerformSave()
        {
            var macro = Current.Services.MacroService.GetByAlias(Alias);
            if (macro == null)
            {
                macro = new Macro(Alias, Alias, string.Empty, MacroTypes.Unknown);
                Current.Services.MacroService.Save(macro);
            }
            _returnUrl = string.Format("developer/Macros/editMacro.aspx?macroID={0}", macro.Id);
            return true;
        }

        public override bool PerformDelete()
        {
            var macro = Current.Services.MacroService.GetById(ParentID);
            if (macro != null)
                Current.Services.MacroService.Delete(macro);
            return true;
        }

        private string _returnUrl = "";

        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return Constants.Applications.Packages.ToString(); }
        }
    }
}
