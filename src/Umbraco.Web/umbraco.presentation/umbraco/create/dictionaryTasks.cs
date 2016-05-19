using System;
using Umbraco.Core.Logging;
using Umbraco.Web.UI;
using Umbraco.Core;
using Umbraco.Web._Legacy.UI;

namespace umbraco
{
    public class dictionaryTasks : LegacyDialogTask
    {
        public override bool PerformSave()
        {
            //check to see if key is already there
            if (ApplicationContext.Current.Services.LocalizationService.DictionaryItemExists(Alias))
                return false;

            // Create new dictionary item if name no already exist
            if (ParentID > 0)
            {
                var di = ApplicationContext.Current.Services.LocalizationService.GetDictionaryItemById(ParentID);
                if (di == null) throw new NullReferenceException("No dictionary item found by id " + ParentID);                
                var item = ApplicationContext.Current.Services.LocalizationService.CreateDictionaryItemWithIdentity(Alias, di.Key);                
                _returnUrl = string.Format("settings/editDictionaryItem.aspx?id={0}", item.Id);
            }
            else
            {
                var item = ApplicationContext.Current.Services.LocalizationService.CreateDictionaryItemWithIdentity(Alias, null);                
                _returnUrl = string.Format("settings/editDictionaryItem.aspx?id={0}", item.Id);
            }
            return true;
        }

        public override bool PerformDelete()
        {
			LogHelper.Debug<dictionaryTasks>(TypeID + " " + ParentID + " deleting " + Alias);
            var di = ApplicationContext.Current.Services.LocalizationService.GetDictionaryItemById(ParentID);
            if (di == null) return true;

            ApplicationContext.Current.Services.LocalizationService.Delete(di);            
            return true;
        }

        private string _returnUrl = "";

        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return Constants.Applications.Settings.ToString(); }
        }
    }
}
