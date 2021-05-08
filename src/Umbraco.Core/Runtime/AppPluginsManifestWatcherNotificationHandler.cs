using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Manifest;

namespace Umbraco.Cms.Core.Runtime
{
    /// <summary>
    /// Starts monitoring AppPlugins directory during debug runs, to restart site when a plugin manifest changes.
    /// </summary>
    public sealed class AppPluginsManifestWatcherNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartingNotification>
    {
        private readonly ManifestWatcher _manifestWatcher;
        private readonly IHostingEnvironment _hostingEnvironment;

        public AppPluginsManifestWatcherNotificationHandler(ManifestWatcher manifestWatcher, IHostingEnvironment hostingEnvironment)
        {
            _manifestWatcher = manifestWatcher ?? throw new ArgumentNullException(nameof(manifestWatcher));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        }

        public Task HandleAsync(UmbracoApplicationStartingNotification notification, CancellationToken cancellationToken)
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
