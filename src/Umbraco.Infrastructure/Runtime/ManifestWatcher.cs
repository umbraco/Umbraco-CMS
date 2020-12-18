using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Hosting;
using Umbraco.Net;

namespace Umbraco.Infrastructure.Runtime
{
    public sealed class ManifestWatcher :
        INotificationHandler<UmbracoApplicationStarting>,
        INotificationHandler<UmbracoApplicationStopping>
    {
        private readonly IHostingEnvironment _hosting;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUmbracoApplicationLifetime _umbracoApplicationLifetime;

        // if configured and in debug mode, a ManifestWatcher watches App_Plugins folders for
        // package.manifest chances and restarts the application on any change
        private Core.Manifest.ManifestWatcher _mw;

        public ManifestWatcher(IHostingEnvironment hosting, ILoggerFactory loggerFactory, IHostingEnvironment hostingEnvironment, IUmbracoApplicationLifetime umbracoApplicationLifetime)
        {
            _hosting = hosting;
            _loggerFactory = loggerFactory;
            _hostingEnvironment = hostingEnvironment;
            _umbracoApplicationLifetime = umbracoApplicationLifetime;
        }

        public Task HandleAsync(UmbracoApplicationStarting notification, CancellationToken cancellationToken)
        {
            if (_hosting.IsDebugMode == false)
            {
                return Task.CompletedTask;
            }

            var appPlugins = _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.AppPlugins);
            if (Directory.Exists(appPlugins) == false)
            {
                return Task.CompletedTask;
            }

            _mw = new Core.Manifest.ManifestWatcher(_loggerFactory.CreateLogger<Core.Manifest.ManifestWatcher>(), _umbracoApplicationLifetime);
            _mw.Start(Directory.GetDirectories(appPlugins));

            return Task.CompletedTask;
        }

        public Task HandleAsync(UmbracoApplicationStopping notification, CancellationToken cancellationToken)
        {
            if (_mw == null)
            {
                return Task.CompletedTask;
            }

            _mw.Dispose();
            _mw = null;

            return Task.CompletedTask;
        }
    }
}
