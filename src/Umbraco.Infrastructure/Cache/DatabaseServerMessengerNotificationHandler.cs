using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Sync;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;
using Umbraco.Core.Sync;
using Umbraco.Web;
using Umbraco.Web.Cache;
using Umbraco.Web.Routing;

namespace Umbraco.Infrastructure.Cache
{
    /// <summary>
    /// Ensures that distributed cache events are setup and the <see cref="IServerMessenger"/> is initialized
    /// </summary>
    public sealed class DatabaseServerMessengerNotificationHandler : INotificationHandler<UmbracoApplicationStarting>, INotificationHandler<UmbracoRequestEnd>
    {
        private readonly IServerMessenger _messenger;
        private readonly IUmbracoDatabaseFactory _databaseFactory;
        private readonly IDistributedCacheBinder _distributedCacheBinder;
        private readonly ILogger<DatabaseServerMessengerNotificationHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseServerMessengerNotificationHandler"/> class.
        /// </summary>
        public DatabaseServerMessengerNotificationHandler(
            IServerMessenger serverMessenger,
            IUmbracoDatabaseFactory databaseFactory,
            IDistributedCacheBinder distributedCacheBinder,
            ILogger<DatabaseServerMessengerNotificationHandler> logger)
        {
            _databaseFactory = databaseFactory;
            _distributedCacheBinder = distributedCacheBinder;
            _logger = logger;
            _messenger = serverMessenger;
        }

        /// <inheritdoc/>
        public void Handle(UmbracoApplicationStarting notification)
        {
            if (_databaseFactory.CanConnect == false)
            {
                _logger.LogWarning("Cannot connect to the database, distributed calls will not be enabled for this server.");
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
