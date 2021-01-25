using Microsoft.Extensions.Logging;
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
    public sealed class DatabaseServerMessengerNotificationHandler : INotificationHandler<UmbracoApplicationStarting>
    {
        private readonly IServerMessenger _messenger;
        private readonly IRequestAccessor _requestAccessor;
        private readonly IUmbracoDatabaseFactory _databaseFactory;
        private readonly IDistributedCacheBinder _distributedCacheBinder;
        private readonly ILogger<DatabaseServerMessengerNotificationHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseServerMessengerNotificationHandler"/> class.
        /// </summary>
        public DatabaseServerMessengerNotificationHandler(
            IServerMessenger serverMessenger,
            IRequestAccessor requestAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            IDistributedCacheBinder distributedCacheBinder,
            ILogger<DatabaseServerMessengerNotificationHandler> logger)
        {
            _requestAccessor = requestAccessor;
            _databaseFactory = databaseFactory;
            _distributedCacheBinder = distributedCacheBinder;
            _logger = logger;
            _messenger = serverMessenger;
        }

        /// <inheritdoc/>
        public void Handle(UmbracoApplicationStarting notification)
        {
            // The scheduled tasks - TouchServerTask and InstructionProcessTask - run as .NET Core hosted services.
            // The former (as well as other hosted services that run outside of an HTTP request context) depends on the application URL
            // being available (via IRequestAccessor), which can only be retrieved within an HTTP request (unless it's explicitly configured).
            // Hence we hook up a one-off task on an HTTP request to ensure this is retrieved, which caches the value and makes it available
            // for the hosted services to use when the HTTP request is not available.
            _requestAccessor.RouteAttempt += EnsureApplicationUrlOnce;
            _requestAccessor.EndRequest += EndRequest;

            Startup();
        }

        private void Startup()
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

        // TODO: I don't really know or think that the Application Url plays a role anymore with the DB dist cache,
        // this might be really old stuff. I 'think' all this is doing is ensuring that the IRequestAccessor.GetApplicationUrl
        // is definitely called during the first request. If that is still required, that logic doesn't belong here. That logic
        // should be part of it's own service/middleware. There's also TODO notes within IRequestAccessor.GetApplicationUrl directly
        // mentioning that the property doesn't belong on that service either. This should be investigated and resolved in a separate task.
        private void EnsureApplicationUrlOnce(object sender, RoutableAttemptEventArgs e)
        {
            if (e.Outcome == EnsureRoutableOutcome.IsRoutable || e.Outcome == EnsureRoutableOutcome.NotDocumentRequest)
            {
                _requestAccessor.RouteAttempt -= EnsureApplicationUrlOnce;
                EnsureApplicationUrl();
            }
        }

        // By retrieving the application URL within the context of a request (as we are here in responding
        // to the IRequestAccessor's RouteAttempt event), we'll get it from the HTTP context and save it for
        // future requests that may not be within an HTTP request (e.g. from hosted services).
        private void EnsureApplicationUrl() => _requestAccessor.GetApplicationUrl();

        /// <summary>
        /// Clear the batch on end request
        /// </summary>
        private void EndRequest(object sender, UmbracoRequestEventArgs e) => _messenger?.SendMessages();
    }
}
