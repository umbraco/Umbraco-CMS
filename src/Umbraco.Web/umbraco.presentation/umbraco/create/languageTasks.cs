using System.Globalization;
using Umbraco.Web.UI;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Composing;
using Umbraco.Web._Legacy.UI;

namespace umbraco
{
    public class languageTasks : LegacyDialogTask
    {

        public override bool PerformSave()
        {
            //cms.businesslogic.language.Language.MakeNew(Alias);
            var culture = new CultureInfo(Alias);
            var l = new Language(Alias) { CultureName = culture.DisplayName };
            Current.Services.LocalizationService.Save(l);
            return true;
        }

        public override bool PerformDelete()
        {
            //new cms.businesslogic.language.Language(ParentID).Delete();
            var l = Current.Services.LocalizationService.GetLanguageById(ParentID);
            if (l != null)
                Current.Services.LocalizationService.Delete(l);
            return false;
        }

        public override string ReturnUrl
        {
            get { return string.Empty; }
        }

        public override string AssignedApp
        {
            get { return Constants.Applications.Settings.ToString(); }
        }
    }

}
