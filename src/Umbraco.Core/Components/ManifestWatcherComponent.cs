using System;
using System.IO;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.Components
{
    public class ManifestWatcherComponent : IComponent, IDisposable
    {
        // if configured and in debug mode, a ManifestWatcher watches App_Plugins folders for
        // package.manifest chances and restarts the application on any change
        private ManifestWatcher _mw;

        public ManifestWatcherComponent(IRuntimeState runtimeState, ILogger logger)
        {
            if (runtimeState.Debug == false) return;

            //if (ApplicationContext.Current.IsConfigured == false || GlobalSettings.DebugMode == false)
            //    return;

            var appPlugins = IOHelper.MapPath("~/App_Plugins/");
            if (Directory.Exists(appPlugins) == false) return;

            _mw = new ManifestWatcher(logger);
            _mw.Start(Directory.GetDirectories(appPlugins));
        }

        public void Dispose()
        {
            if (_mw == null) return;

            _mw.Dispose();
            _mw = null;
        }
    }
}
