using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core.Events;
using Umbraco.Core.Hosting;
using Umbraco.Core.Manifest;

namespace Umbraco.Core.Runtime
{
    /// <summary>
    /// Starts monitoring AppPlugins directory during debug runs, to restart site when a plugin manifest changes.
    /// </summary>
    public sealed class AppPluginsManifestWatcherNotificationHandler : INotificationHandler<UmbracoApplicationStarting>
    {
        private readonly ManifestWatcher _manifestWatcher;
        private readonly IHostingEnvironment _hostingEnvironment;

        public AppPluginsManifestWatcherNotificationHandler(ManifestWatcher manifestWatcher, IHostingEnvironment hostingEnvironment)
        {
            _manifestWatcher = manifestWatcher ?? throw new ArgumentNullException(nameof(manifestWatcher));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        }

        public Task HandleAsync(UmbracoApplicationStarting notification, CancellationToken cancellationToken)
        {
            if (!_hostingEnvironment.IsDebugMode)
            {
                return Task.CompletedTask;
            }

            var appPlugins = _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.AppPlugins);

            if (!Directory.Exists(appPlugins))
            {
                return Task.CompletedTask;
            }

            _manifestWatcher.Start(Directory.GetDirectories(appPlugins));

            return Task.CompletedTask;
        }
    }
}
