using Umbraco.Web.UI;
using Umbraco.Core;
using Umbraco.Web._Legacy.UI;

namespace umbraco
{
    public class languageTasks : LegacyDialogTask
    {
       
        public override bool PerformSave()
        {
            cms.businesslogic.language.Language.MakeNew(Alias);
            return true;
        }

        public override bool PerformDelete()
        {
            new cms.businesslogic.language.Language(ParentID).Delete();
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
