using System;
using System.IO;
using Umbraco.Core.IO;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.Strategies
{
    public sealed class ManifestWatcherHandler : ApplicationEventHandler
    {
        private ManifestWatcher _mw;

        protected override void ApplicationInitialized(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            UmbracoApplicationBase.ApplicationEnd += app_ApplicationEnd;
        }

        void app_ApplicationEnd(object sender, EventArgs e)
        {
            if (_mw != null)
            {
                _mw.Dispose();
            }
        }

        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            _mw = new ManifestWatcher(applicationContext.ProfilingLogger.Logger);
            _mw.Start(Directory.GetDirectories(IOHelper.MapPath("~/App_Plugins/")));
        }
    }
}