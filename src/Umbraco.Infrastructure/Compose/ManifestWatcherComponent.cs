using System.IO;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Manifest;
using Umbraco.Net;

namespace Umbraco.Core.Compose
{
    public sealed class ManifestWatcherComponent : IComponent
    {
        private readonly IHostingEnvironment _hosting;
        private readonly ILogger _logger;
        private readonly IIOHelper _ioHelper;
        private readonly IUmbracoApplicationLifetime _umbracoApplicationLifetime;

        // if configured and in debug mode, a ManifestWatcher watches App_Plugins folders for
        // package.manifest chances and restarts the application on any change
        private ManifestWatcher _mw;

        public ManifestWatcherComponent(IHostingEnvironment hosting, ILogger logger, IIOHelper ioHelper, IUmbracoApplicationLifetime umbracoApplicationLifetime)
        {
            _hosting = hosting;
            _logger = logger;
            _ioHelper = ioHelper;
            _umbracoApplicationLifetime = umbracoApplicationLifetime;
        }

        public void Initialize()
        {
            if (_hosting.IsDebugMode == false) return;

            //if (ApplicationContext.Current.IsConfigured == false || GlobalSettings.DebugMode == false)
            //    return;

            var appPlugins = _ioHelper.MapPath("~/App_Plugins/");
            if (Directory.Exists(appPlugins) == false) return;

            _mw = new ManifestWatcher(_logger, _umbracoApplicationLifetime);
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
