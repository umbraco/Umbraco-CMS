using System.IO;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class ManifestWatcherComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        // if configured and in debug mode, a ManifestWatcher watches App_Plugins folders for
        // package.manifest chances and restarts the application on any change
        private ManifestWatcher _mw;

        public class Initializer : IUmbracoInitializer
        {
            private readonly IRuntimeState _runtimeState;
            private readonly ILogger _logger;

            public Initializer(IRuntimeState runtimeState, ILogger logger)
            {
                _runtimeState = runtimeState;
                _logger = logger;
            }

            public void Initialize()
            {
                if (_runtimeState.Debug == false) return;

                //if (ApplicationContext.Current.IsConfigured == false || GlobalSettings.DebugMode == false)
                //    return;

                var appPlugins = IOHelper.MapPath("~/App_Plugins/");
                if (Directory.Exists(appPlugins) == false) return;

                _mw = new ManifestWatcher(_logger);
                _mw.Start(Directory.GetDirectories(appPlugins));
            }
        }

        public void Initialize(IRuntimeState runtime, ILogger logger)
        {
            if (runtime.Debug == false) return;

            //if (ApplicationContext.Current.IsConfigured == false || GlobalSettings.DebugMode == false)
            //    return;

            var appPlugins = IOHelper.MapPath("~/App_Plugins/");
            if (Directory.Exists(appPlugins) == false) return;

            _mw = new ManifestWatcher(logger);
            _mw.Start(Directory.GetDirectories(appPlugins));
        }

        public override void Terminate()
        {
            _mw?.Dispose();
            _mw = null;
        }
    }
}
