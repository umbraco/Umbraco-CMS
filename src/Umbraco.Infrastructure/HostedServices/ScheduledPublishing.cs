using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Umbraco.Web;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Infrastructure.HostedServices
{
    /// <summary>
    /// Hosted service implementation for scheduled publishing feature.
    /// </summary>
    /// <remarks>
    /// Runs only on non-replica servers.</remarks>
    [UmbracoVolatile]
    public class ScheduledPublishing : RecurringHostedServiceBase
    {
        private readonly IContentService _contentService;
        private readonly ILogger<ScheduledPublishing> _logger;
        private readonly IMainDom _mainDom;
        private readonly IRuntimeState _runtime;
        private readonly IServerMessenger _serverMessenger;
        private readonly IBackOfficeSecurityFactory _backofficeSecurityFactory;
        private readonly IServerRegistrar _serverRegistrar;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public ScheduledPublishing(
            IRuntimeState runtime, IMainDom mainDom, IServerRegistrar serverRegistrar, IContentService contentService,
            IUmbracoContextFactory umbracoContextFactory, ILogger<ScheduledPublishing> logger, IServerMessenger serverMessenger, IBackOfficeSecurityFactory backofficeSecurityFactory)
            : base(TimeSpan.FromMinutes(1), DefaultDelay)
        {
            _runtime = runtime;
            _mainDom = mainDom;
            _serverRegistrar = serverRegistrar;
            _contentService = contentService;
            _umbracoContextFactory = umbracoContextFactory;
            _logger = logger;
            _serverMessenger = serverMessenger;
            _backofficeSecurityFactory = backofficeSecurityFactory;
        }

        internal override async Task PerformExecuteAsync(object state)
        {
            if (Suspendable.ScheduledPublishing.CanRun == false)
            {
                return;
            }

            switch (_serverRegistrar.GetCurrentServerRole())
            {
                case ServerRole.Replica:
                    _logger.LogDebug("Does not run on replica servers.");
                    return;
                case ServerRole.Unknown:
                    _logger.LogDebug("Does not run on servers with unknown role.");
                    return;
            }

            // Ensure we do not run if not main domain, but do NOT lock it
            if (_mainDom.IsMainDom == false)
            {
                _logger.LogDebug("Does not run if not MainDom.");
                return;
            }

            // Do NOT run publishing if not properly running
            if (_runtime.Level != RuntimeLevel.Run)
            {
                _logger.LogDebug("Does not run if run level is not Run.");
                return;
            }

            try
            {
                // We don't need an explicit scope here because PerformScheduledPublish creates it's own scope
                // so it's safe as it will create it's own ambient scope.
                // Ensure we run with an UmbracoContext, because this will run in a background task,
                // and developers may be using the UmbracoContext in the event handlers.

                // TODO: or maybe not, CacheRefresherComponent already ensures a context when handling events
                // - UmbracoContext 'current' needs to be refactored and cleaned up
                // - batched messenger should not depend on a current HttpContext
                //    but then what should be its "scope"? could we attach it to scopes?
                // - and we should definitively *not* have to flush it here (should be auto)
                //
                _backofficeSecurityFactory.EnsureBackOfficeSecurity();
                using var contextReference = _umbracoContextFactory.EnsureUmbracoContext();
                try
                {
                    // Run
                    var result = _contentService.PerformScheduledPublish(DateTime.Now);
                    foreach (var grouped in result.GroupBy(x => x.Result))
                    {
                        _logger.LogInformation(
                            "Scheduled publishing result: '{StatusCount}' items with status {Status}",
                            grouped.Count(), grouped.Key);
                    }
                }
                finally
                {
                    // If running on a temp context, we have to flush the messenger
                    if (contextReference.IsRoot && _serverMessenger is IBatchedDatabaseServerMessenger m)
                    {
                        m.FlushBatch();
                    }
                }
            }
            catch (Exception ex)
            {
                // important to catch *everything* to ensure the task repeats
                _logger.LogError(ex, "Failed.");
            }

            return;
        }
    }
}
