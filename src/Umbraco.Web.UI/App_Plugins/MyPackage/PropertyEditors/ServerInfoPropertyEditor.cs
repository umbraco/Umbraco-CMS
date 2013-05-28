using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Belle.System.PropertyEditors;

namespace Umbraco.Belle.App_Plugins.MyPackage.PropertyEditors
{
    [PropertyEditor("AD056473-492B-47F8-9613-5A4936666C67", "Server Info")]
    public class ServerInfoPropertyEditor : PropertyEditor
    {
        protected override ValueEditor CreateValueEditor()
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException("This property editor only works in a web context");
            }
            
            var urlHelper = new UrlHelper(new RequestContext(new HttpContextWrapper(HttpContext.Current), new RouteData()));

            return new ValueEditor(urlHelper.Action("ServerEnvironment", "ServerSidePropertyEditors", new {area = "MyPackage"}));
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new PreValueEditor();
        }
    }
}