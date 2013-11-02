using Umbraco.Core.Configuration;
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
            mediaType.IconUrl = ".sprTreeFolder";
           
            if (ParentID != -1)
            {
                mediaType.MasterContentType = ParentID;
                mediaType.Save();
            }

            _returnUrl = string.Format("settings/editMediaType.aspx?id={0}", mediaType.Id);
            return true;
        }

        public override bool PerformDelete()
        {
            var mediaType = ApplicationContext.Current.Services.ContentTypeService.GetMediaType(ParentID);
            if (mediaType != null)
            {
                ApplicationContext.Current.Services.ContentTypeService.Delete(mediaType);
            }
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
