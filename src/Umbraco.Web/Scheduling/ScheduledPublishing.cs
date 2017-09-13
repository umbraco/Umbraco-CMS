using System;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;

namespace Umbraco.Web.Scheduling
{
    internal class ScheduledPublishing : RecurringTaskBase
    {
        private readonly IRuntimeState _runtime;
        private readonly IContentService _contentService;
        private readonly ILogger _logger;

        public ScheduledPublishing(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
            IRuntimeState runtime, IContentService contentService, ILogger logger)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _runtime = runtime;
            _contentService = contentService;
            _logger = logger;
        }

        public override async Task<bool> PerformRunAsync(CancellationToken token)
        {
            switch (_runtime.ServerRole)
            {
                case ServerRole.Slave:
                    _logger.Debug<ScheduledPublishing>("Does not run on slave servers.");
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

            try
            {
                // DO not run publishing if content is re-loading
                if (_runtime.Level != RuntimeLevel.Run)
                {
                    var publisher = new ScheduledPublisher(_contentService, _logger);
                    var count = publisher.CheckPendingAndProcess();
                    _logger.Warn<ScheduledPublishing>("No url for service (yet), skip.");
                }
            }
            catch (Exception e)
            {
                _logger.Error<ScheduledPublishing>("Failed.", e);
            }

            return true; // repeat
        }

        public override bool IsAsync => true;
    }
}
