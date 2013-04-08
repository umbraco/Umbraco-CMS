using System;
using System.Data;
using System.Web.Security;
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
            Log.Add(LogTypes.Delete, User, d.Id, "");

            d.delete();
            return true;

        }

    }
}
