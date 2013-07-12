using System;
using System.Data;
using System.Web.Security;
using Umbraco.Core.Logging;
using Umbraco.Web.UI;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using umbraco.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class mediaTasks : LegacyDialogTask
    {
        private string _returnUrl = "";

        public override string ReturnUrl
        {
            get { return _returnUrl; }
        }

        public override string AssignedApp
        {
            get { return DefaultApps.media.ToString(); }
        }

        public override bool PerformSave()
        {
            var dt = new cms.businesslogic.media.MediaType(TypeID);
            var m = cms.businesslogic.media.Media.MakeNew(Alias, dt, User, ParentID);
            _returnUrl = "editMedia.aspx?id=" + m.Id.ToString() + "&isNew=true";

            return true;
        }

        public override bool PerformDelete()
        {
            var d = new cms.businesslogic.media.Media(ParentID);

            // Log
            LogHelper.Debug<mediaTasks>(string.Format("Delete media item {0} by user {1}", d.Id, User.GetCurrent().Id));

            d.delete();
            return true;

        }

    }
}
