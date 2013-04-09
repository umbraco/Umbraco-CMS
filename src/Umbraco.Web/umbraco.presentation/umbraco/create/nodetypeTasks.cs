using System;
using System.Configuration;
using System.Data;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Web.UI;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using umbraco.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class nodetypeTasks : LegacyDialogTask
    {
       
        public override bool PerformSave()
        {
            var dt = cms.businesslogic.web.DocumentType.MakeNew(User, Alias.Replace("'", "''"));
            dt.IconUrl = UmbracoSettings.IconPickerBehaviour == IconPickerBehaviour.HideFileDuplicates ? ".sprTreeFolder" : "folder.gif";

            // Create template?
            if (ParentID == 1)
            {
                cms.businesslogic.template.Template[] t = { cms.businesslogic.template.Template.MakeNew(Alias, User) };
                dt.allowedTemplates = t;
                dt.DefaultTemplate = t[0].Id;
            }

            // Master Content Type?
            if (TypeID != 0)
            {
                dt.MasterContentType = TypeID;
            }

            _returnUrl = "settings/editNodeTypeNew.aspx?id=" + dt.Id.ToString();

            return true;
        }

        public override bool PerformDelete()
        {
            new cms.businesslogic.web.DocumentType(ParentID).delete();

            //after a document type is deleted, we clear the cache, as some content will now have disappeared.
            library.RefreshContent();

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
