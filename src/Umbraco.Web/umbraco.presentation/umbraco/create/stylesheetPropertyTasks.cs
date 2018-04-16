using System;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.UI;
using Umbraco.Web._Legacy.UI;

namespace umbraco
{
    public class stylesheetPropertyTasks : LegacyDialogTask
    {
        public override bool PerformSave()
        {
            var stylesheetName = AdditionalValues["nodeId"].ToString();

            var s = Current.Services.FileService.GetStylesheetByName(stylesheetName.EnsureEndsWith(".css"));
            s.AddProperty(new StylesheetProperty(Alias, "." + Alias.ToSafeAlias(), ""));
            Current.Services.FileService.SaveStylesheet(s);

            // SJ - Note: The Alias is NOT in fact the alias but the name of the new property, need to UrlEncode it!
            _returnUrl = string.Format("settings/stylesheet/property/EditStyleSheetProperty.aspx?id={0}&prop={1}", HttpUtility.UrlEncode(s.Path), HttpUtility.UrlEncode(Alias));
            return true;
        }

        public override bool PerformDelete()
        {
            var parts = Alias.Split('_');

            var stylesheet = Current.Services.FileService.GetStylesheetByName(parts[0].EnsureEndsWith(".css"));
            if (stylesheet == null) throw new InvalidOperationException("No stylesheet found by name: " + parts[0]);

            var property = HttpUtility.UrlDecode(parts[1]);
            var prop = stylesheet.Properties.FirstOrDefault(x => x.Name == property);
            if (prop == null) throw new InvalidOperationException("No stylesheet property found by name: " + property);

            stylesheet.RemoveProperty(prop.Name);

            Current.Services.FileService.SaveStylesheet(stylesheet);

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
