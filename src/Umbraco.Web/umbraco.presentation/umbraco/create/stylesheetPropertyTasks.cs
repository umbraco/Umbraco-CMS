using System;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.UI;

namespace umbraco
{
    public class stylesheetPropertyTasks : LegacyDialogTask
    {

        public override bool PerformSave()
        {
            var stylesheetName = AdditionalValues["nodeId"].ToString();

            var s = Umbraco.Core.ApplicationContext.Current.Services.FileService.GetStylesheetByName(stylesheetName.EnsureEndsWith(".css"));
            s.AddProperty(new StylesheetProperty(Alias, "." + Alias.ToSafeAlias(), ""));
            Umbraco.Core.ApplicationContext.Current.Services.FileService.SaveStylesheet(s);

            _returnUrl = string.Format("settings/stylesheet/property/EditStyleSheetProperty.aspx?id={0}&prop={1}", HttpUtility.UrlEncode(s.Path), Alias);
            return true;
        }

        public override bool PerformDelete()
        {
            var parts = Alias.Split('_');

            var stylesheet = Umbraco.Core.ApplicationContext.Current.Services.FileService.GetStylesheetByName(parts[0].EnsureEndsWith(".css"));
            if (stylesheet == null) throw new InvalidOperationException("No stylesheet found by name: " + parts[0]);

            var prop = stylesheet.Properties.FirstOrDefault(x => x.Name == parts[1]);
            if (prop == null) throw new InvalidOperationException("No stylesheet property found by name: " + parts[1]);

            stylesheet.RemoveProperty(prop.Name);

            Umbraco.Core.ApplicationContext.Current.Services.FileService.SaveStylesheet(stylesheet);

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
