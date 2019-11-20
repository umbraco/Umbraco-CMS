using System.IO;
using Umbraco.Core.Composing;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.Compose
{
    public sealed class ManifestWatcherComponent : IComponent
    {
        private readonly IRuntimeState _runtimeState;
        private readonly ILogger _logger;
        private readonly IIOHelper _ioHelper;

        // if configured and in debug mode, a ManifestWatcher watches App_Plugins folders for
        // package.manifest chances and restarts the application on any change
        private ManifestWatcher _mw;

        public ManifestWatcherComponent(IRuntimeState runtimeState, ILogger logger, IIOHelper ioHelper)
        {
            _runtimeState = runtimeState;
            _logger = logger;
            _ioHelper = ioHelper;
        }

        public void Initialize()
        {
            if (_runtimeState.Debug == false) return;

            //if (ApplicationContext.Current.IsConfigured == false || GlobalSettings.DebugMode == false)
            //    return;

            var appPlugins = _ioHelper.MapPath("~/App_Plugins/");
            if (Directory.Exists(appPlugins) == false) return;

            _mw = new ManifestWatcher(_logger);
            _mw.Start(Directory.GetDirectories(appPlugins));
        }

        public void Terminate()
        {
            if (_mw == null) return;

            _mw.Dispose();
            _mw = null;
        }
    }
}
