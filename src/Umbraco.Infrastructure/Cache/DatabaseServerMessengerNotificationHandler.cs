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
        private readonly IUmbracoDatabaseFactory _databaseFactory;
        private readonly ILogger<DatabaseServerMessengerNotificationHandler> _logger;
        private readonly IRuntimeState _runtimeState;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseServerMessengerNotificationHandler"/> class.
        /// </summary>
        public DatabaseServerMessengerNotificationHandler(
            IServerMessenger serverMessenger,
            IUmbracoDatabaseFactory databaseFactory,
            ILogger<DatabaseServerMessengerNotificationHandler> logger,
            IRuntimeState runtimeState)
        {
            _databaseFactory = databaseFactory;
            _logger = logger;
            _messenger = serverMessenger;
            _runtimeState = runtimeState;
        }

        /// <inheritdoc/>
        public void Handle(UmbracoApplicationStarting notification)
        {
            if (_databaseFactory.CanConnect == false)
			{
				_logger.LogWarning("Cannot connect to the database, distributed calls will not be enabled for this server.");
			}
            else if (_runtimeState.Level != RuntimeLevel.Run)
            {
                _logger.LogWarning("Distributed calls are not available outside the Run runtime level");
            }
			else
            {
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
