using Umbraco.Web.UI;
using Umbraco.Core;
using umbraco.BusinessLogic;

namespace umbraco
{
    public class MediaTypeTasks : LegacyDialogTask
    {
       
        public override bool PerformSave()
        { 

            var mediaType = cms.businesslogic.media.MediaType.MakeNew(User, Alias.Replace("'", "''"));
            mediaType.IconUrl = UmbracoSettings.IconPickerBehaviour == IconPickerBehaviour.HideFileDuplicates ? ".sprTreeFolder" : "folder.gif";

            _returnUrl = string.Format("settings/editMediaType.aspx?id={0}", mediaType.Id);
            return true;
        }

        public override bool PerformDelete()
        {
            new cms.businesslogic.media.MediaType(ParentID).delete();
            return false;
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
