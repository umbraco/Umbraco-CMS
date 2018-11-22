using System;
using System.Data;
using System.Web.Security;
using Umbraco.Core.Logging;
using Umbraco.Web.UI;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class dictionaryTasks : LegacyDialogTask
    {
        public override bool PerformSave()
        {
            //check to see if key is already there
            if (cms.businesslogic.Dictionary.DictionaryItem.hasKey(Alias))
                return false;

            // Create new dictionary item if name no already exist
            if (ParentID > 0)
            {
                var id = cms.businesslogic.Dictionary.DictionaryItem.addKey(Alias, "", new cms.businesslogic.Dictionary.DictionaryItem(ParentID).key);
                _returnUrl = string.Format("settings/editDictionaryItem.aspx?id={0}", id);
            }
            else
            {
                var id = cms.businesslogic.Dictionary.DictionaryItem.addKey(Alias, "");
                _returnUrl = string.Format("settings/editDictionaryItem.aspx?id={0}", id);
            }
            return true;
        }

        public override bool PerformDelete()
        {
			LogHelper.Debug<dictionaryTasks>(TypeID.ToString() + " " + ParentID.ToString() + " deleting " + Alias);
            new cms.businesslogic.Dictionary.DictionaryItem(ParentID).delete();
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
