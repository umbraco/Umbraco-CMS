// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Core.Cache
{
    /// <summary>
    /// Ensures that distributed cache events are setup and the <see cref="IServerMessenger"/> is initialized
    /// </summary>
    public sealed class DatabaseServerMessengerNotificationHandler : INotificationHandler<UmbracoApplicationStarting>, INotificationHandler<UmbracoRequestEnd>
    {
        private readonly IServerMessenger _messenger;
        private readonly IRuntimeState _runtimeState;
        private readonly IDistributedCacheBinder _distributedCacheBinder;
        private readonly ILogger<DatabaseServerMessengerNotificationHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseServerMessengerNotificationHandler"/> class.
        /// </summary>
        public DatabaseServerMessengerNotificationHandler(
            IServerMessenger serverMessenger,
            IRuntimeState runtimeState,
            IDistributedCacheBinder distributedCacheBinder,
            ILogger<DatabaseServerMessengerNotificationHandler> logger)
        {
            _distributedCacheBinder = distributedCacheBinder;
            _logger = logger;
            _messenger = serverMessenger;
            _runtimeState = runtimeState;
        }

        /// <inheritdoc/>
        public void Handle(UmbracoApplicationStarting notification)
        {
            if (_runtimeState.Level == RuntimeLevel.Install)
            {
                _logger.LogWarning("Disabling distributed calls during install");
            }
            else
            {
                _distributedCacheBinder.BindEvents();

                // Sync on startup, this will run through the messenger's initialization sequence
                _messenger?.Sync();
            }
        }

        /// <summary>
        /// Clear the batch on end request
        /// </summary>
        public void Handle(UmbracoRequestEnd notification) => _messenger?.SendMessages();
    }
}
