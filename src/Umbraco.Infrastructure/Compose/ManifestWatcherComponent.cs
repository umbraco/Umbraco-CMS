using System.IO;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Composing;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Manifest;
using Umbraco.Net;

namespace Umbraco.Core.Compose
{
    public sealed class ManifestWatcherComponent : IComponent
    {
        private readonly IHostingEnvironment _hosting;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUmbracoApplicationLifetime _umbracoApplicationLifetime;

        // if configured and in debug mode, a ManifestWatcher watches App_Plugins folders for
        // package.manifest chances and restarts the application on any change
        private ManifestWatcher _mw;

        public ManifestWatcherComponent(IHostingEnvironment hosting, ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IUmbracoApplicationLifetime umbracoApplicationLifetime)
        {
            _hosting = hosting;
            _loggerFactory = loggerFactory;
            _hostingEnvironment = hostingEnvironment;
            _umbracoApplicationLifetime = umbracoApplicationLifetime;
        }

        public void Initialize()
        {
            if (_hosting.IsDebugMode == false) return;

            //if (ApplicationContext.Current.IsConfigured == false || GlobalSettings.DebugMode == false)
            //    return;

            var appPlugins = _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.AppPlugins);
            if (Directory.Exists(appPlugins) == false) return;

            _mw = new ManifestWatcher(_loggerFactory.CreateLogger<ManifestWatcher>(), _umbracoApplicationLifetime);
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
