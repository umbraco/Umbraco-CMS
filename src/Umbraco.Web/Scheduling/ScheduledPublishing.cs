using System;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;

namespace Umbraco.Web.Scheduling
{
    internal class ScheduledPublishing : RecurringTaskBase
    {
        private readonly IRuntimeState _runtime;
        private readonly IContentService _contentService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly ILogger _logger;

        public ScheduledPublishing(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
            IRuntimeState runtime, IContentService contentService, IUmbracoContextFactory umbracoContextFactory, ILogger logger)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _runtime = runtime;
            _contentService = contentService;
            _umbracoContextFactory = umbracoContextFactory;
            _logger = logger;
        }

        public override bool PerformRun()
        {
            if (Suspendable.ScheduledPublishing.CanRun == false)
                return true; // repeat, later

            switch (_runtime.ServerRole)
            {
                case ServerRole.Replica:
                    _logger.Debug<ScheduledPublishing>("Does not run on replica servers.");
                    return true; // DO repeat, server role can change
                case ServerRole.Unknown:
                    _logger.Debug<ScheduledPublishing>("Does not run on servers with unknown role.");
                    return true; // DO repeat, server role can change
            }

            // ensure we do not run if not main domain, but do NOT lock it
            if (_runtime.IsMainDom == false)
            {
                _logger.Debug<ScheduledPublishing>("Does not run if not MainDom.");
                return false; // do NOT repeat, going down
            }

            // do NOT run publishing if not properly running
            if (_runtime.Level != RuntimeLevel.Run)
            {
                _logger.Debug<ScheduledPublishing>("Does not run if run level is not Run.");
                return true; // repeat/wait
            }

            try
            {
                // We don't need an explicit scope here because PerformScheduledPublish creates it's own scope
                // so it's safe as it will create it's own ambient scope.
                // Ensure we run with an UmbracoContext, because this will run in a background task,
                // and developers may be using the UmbracoContext in the event handlers.

                using var contextReference = _umbracoContextFactory.EnsureUmbracoContext();
                try
                {
                    // run
                    var result = _contentService.PerformScheduledPublish(DateTime.Now);
                    foreach (var grouped in result.GroupBy(x => x.Result))
                        _logger.Info<ScheduledPublishing,int,PublishResultType>("Scheduled publishing result: '{StatusCount}' items with status {Status}", grouped.Count(), grouped.Key);
                }
                finally
                {
                    // if running on a temp context, we have to flush the messenger
                    if (contextReference.IsRoot && Composing.Current.ServerMessenger is BatchedDatabaseServerMessenger m)
                        m.FlushBatch();
                }
            }
            catch (Exception ex)
            {
                // important to catch *everything* to ensure the task repeats
                _logger.Error<ScheduledPublishing>(ex, "Failed.");
            }

            return true; // repeat
        }

        public override bool IsAsync => false;
    }
}
