using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Microsoft.Extensions.Logging;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web.Scheduling
{
    [UmbracoVolatile]
    public class ScheduledPublishing : RecurringTaskBase
    {
        private readonly IContentService _contentService;
        private readonly ILogger<ScheduledPublishing> _logger;
        private readonly IMainDom _mainDom;
        private readonly IRuntimeState _runtime;
        private readonly IServerMessenger _serverMessenger;
        private readonly IBackofficeSecurityFactory _backofficeSecurityFactory;
        private readonly IServerRegistrar _serverRegistrar;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public ScheduledPublishing(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds,
            int periodMilliseconds,
            IRuntimeState runtime, IMainDom mainDom, IServerRegistrar serverRegistrar, IContentService contentService,
            IUmbracoContextFactory umbracoContextFactory, ILogger<ScheduledPublishing> logger, IServerMessenger serverMessenger, IBackofficeSecurityFactory backofficeSecurityFactory)
            : base(runner, delayMilliseconds, periodMilliseconds)
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

        public override bool IsAsync => false;

        public override bool PerformRun()
        {
            if (Suspendable.ScheduledPublishing.CanRun == false)
                return true; // repeat, later

            switch (_serverRegistrar.GetCurrentServerRole())
            {
                case ServerRole.Replica:
                    _logger.LogDebug("Does not run on replica servers.");
                    return true; // DO repeat, server role can change
                case ServerRole.Unknown:
                    _logger.LogDebug("Does not run on servers with unknown role.");
                    return true; // DO repeat, server role can change
            }

            // ensure we do not run if not main domain, but do NOT lock it
            if (_mainDom.IsMainDom == false)
            {
                _logger.LogDebug("Does not run if not MainDom.");
                return false; // do NOT repeat, going down
            }

            // do NOT run publishing if not properly running
            if (_runtime.Level != RuntimeLevel.Run)
            {
                _logger.LogDebug("Does not run if run level is not Run.");
                return true; // repeat/wait
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
                _backofficeSecurityFactory.EnsureBackofficeSecurity();
                using (var contextReference = _umbracoContextFactory.EnsureUmbracoContext())
                {
                    try
                    {
                        // run
                        var result = _contentService.PerformScheduledPublish(DateTime.Now);
                        foreach (var grouped in result.GroupBy(x => x.Result))
                            _logger.LogInformation(
                                "Scheduled publishing result: '{StatusCount}' items with status {Status}",
                                grouped.Count(), grouped.Key);
                    }
                    finally
                    {
                        // if running on a temp context, we have to flush the messenger
                        if (contextReference.IsRoot && _serverMessenger is IBatchedDatabaseServerMessenger m)
                            m.FlushBatch();
                    }
                }
            }
            catch (Exception ex)
            {
                // important to catch *everything* to ensure the task repeats
                _logger.LogError(ex, "Failed.");
            }

            return true; // repeat
        }
    }
}
