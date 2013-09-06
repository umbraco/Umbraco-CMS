using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.UI.App_Plugins.MyPackage.PropertyEditors
{
    [PropertyEditor("MyPackage.ServerInfo", "Server Info")]
    public class ServerInfoPropertyEditor : PropertyEditor
    {
        //cache the URL since these values get called numerous times.
        private static string _viewPath;
        private static ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        protected override ValueEditor CreateValueEditor()
        {
            if (UmbracoContext.Current == null || UmbracoContext.Current.HttpContext == null)
            {
                throw new InvalidOperationException("This property editor only works in an umbraco web context");
            }
            
            using (var lck = new UpgradeableReadLock(_locker))
            {
                if (_viewPath == null)
                {
                    lck.UpgradeToWriteLock();
                    var urlHelper = new UrlHelper(new RequestContext(UmbracoContext.Current.HttpContext, new RouteData()));
                    _viewPath = urlHelper.Action("ServerEnvironment", "ServerSidePropertyEditors", new { area = "MyPackage" });
                }
                return new ValueEditor(_viewPath);    
            }

            
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new PreValueEditor();
        }
    }
}