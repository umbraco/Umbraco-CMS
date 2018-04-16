using System.Web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.UI;
using Umbraco.Web._Legacy.UI;

namespace umbraco
{
    public class StylesheetTasks : LegacyDialogTask
    {
        public override bool PerformSave()
        {
            //normalize path
            Alias = Alias.Replace("/", "\\");

            var sheet = Current.Services.FileService.GetStylesheetByName(Alias);
            if (sheet == null)
            {
                sheet = new Stylesheet(Alias.EnsureEndsWith(".css"));
                Current.Services.FileService.SaveStylesheet(sheet);
            }

            _returnUrl = string.Format("settings/stylesheet/editStylesheet.aspx?id={0}", HttpUtility.UrlEncode(sheet.Path));
            return true;
        }

        public override bool PerformDelete()
        {
            //var s = cms.businesslogic.web.StyleSheet.GetByName(Alias);
            var s = Current.Services.FileService.GetStylesheetByName(Alias);
            //s.delete();
            if (s!=null)
                Current.Services.FileService.DeleteStylesheet(s.Path);
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
