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
            get { return DefaultApps.settings.ToString(); }
        }
    }

}
