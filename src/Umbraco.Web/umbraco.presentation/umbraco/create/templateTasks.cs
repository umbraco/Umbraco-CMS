using System;
using System.Data;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.UI;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.BasePages;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.member;

namespace umbraco
{
    public class templateTasks : LegacyDialogTask
    {
       
        public override bool PerformSave()
        {
            var masterId = ParentID;
            
            var editor = "settings/editTemplate.aspx";
            if (UmbracoConfig.For.UmbracoSettings().Templates.DefaultRenderingEngine == RenderingEngine.Mvc)
                editor = "settings/views/editView.aspx";

            if (masterId > 0)
            {
                var id = cms.businesslogic.template.Template.MakeNew(Alias, User, new cms.businesslogic.template.Template(masterId)).Id;
                _returnUrl = string.Format("{1}?treeType=templates&templateID={0}", id, editor);
            }
            else
            {
                var id = cms.businesslogic.template.Template.MakeNew(Alias, User).Id;
                _returnUrl = string.Format("{1}?treeType=templates&templateID={0}", id, editor);

            }
            return true;
        }

        public override bool PerformDelete()
        {
            new cms.businesslogic.template.Template(ParentID).delete();
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
