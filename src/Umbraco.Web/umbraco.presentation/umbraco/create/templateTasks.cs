using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Composing;
using Umbraco.Web.UI;
using Umbraco.Web._Legacy.UI;

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
                //var id = cms.businesslogic.template.Template.MakeNew(Alias, User, new cms.businesslogic.template.Template(masterId)).Id;
                var master = Current.Services.FileService.GetTemplate(masterId);
                var t = Current.Services.FileService.CreateTemplateWithIdentity(Alias, null, master, User.Id);
                _returnUrl = string.Format("{1}?treeType=templates&templateID={0}", t.Id, editor);
            }
            else
            {
                //var id = cms.businesslogic.template.Template.MakeNew(Alias, User).Id;
                var t = Current.Services.FileService.CreateTemplateWithIdentity(Alias, null, null, User.Id);
                _returnUrl = string.Format("{1}?treeType=templates&templateID={0}", t.Id, editor);

            }
            return true;
        }

        public override bool PerformDelete()
        {
            //new cms.businesslogic.template.Template(ParentID).delete();
            var t = Current.Services.FileService.GetTemplate(ParentID);
            if (t != null) Current.Services.FileService.DeleteTemplate(t.Alias);
            return false;
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
