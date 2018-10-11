using System;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;

namespace Umbraco.Web.Scheduling
{
    internal class LogScrubber : RecurringTaskBase
    {
        private readonly IRuntimeState _runtime;
        private readonly IAuditService _auditService;
        private readonly IUmbracoSettingsSection _settings;
        private readonly ILogger _logger;
        private readonly ProfilingLogger _proflog;
        private readonly IScopeProvider _scopeProvider;

        public LogScrubber(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
            IRuntimeState runtime, IAuditService auditService, IUmbracoSettingsSection settings, IScopeProvider scopeProvider, ILogger logger, ProfilingLogger proflog)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _runtime = runtime;
            _auditService = auditService;
            _settings = settings;
            _scopeProvider = scopeProvider;
            _logger = logger;
            _proflog = proflog;
        }

        // maximum age, in minutes
        private int GetLogScrubbingMaximumAge(IUmbracoSettingsSection settings)
        {
            var maximumAge = 24 * 60; // 24 hours, in minutes
            try
            {
                if (settings.Logging.MaxLogAge > -1)
                    maximumAge = settings.Logging.MaxLogAge;
            }
            catch (Exception ex)
            {
                _logger.Error<LogScrubber>(ex, "Unable to locate a log scrubbing maximum age. Defaulting to 24 hours.");
            }
            return maximumAge;

        }

        public static int GetLogScrubbingInterval(IUmbracoSettingsSection settings, ILogger logger)
        {
            var interval = 4 * 60 * 60 * 1000; // 4 hours, in milliseconds
            try
            {
                if (settings.Logging.CleaningMiliseconds > -1)
                    interval = settings.Logging.CleaningMiliseconds;
            }
            catch (Exception ex)
            {
                logger.Error<LogScrubber>(ex, "Unable to locate a log scrubbing interval. Defaulting to 4 hours.");
            }
            return interval;
        }

        public override bool PerformRun()
        {
            switch (_runtime.ServerRole)
            {
                case ServerRole.Replica:
                    _logger.Debug<LogScrubber>("Does not run on replica servers.");
                    return true; // DO repeat, server role can change
                case ServerRole.Unknown:
                    _logger.Debug<LogScrubber>("Does not run on servers with unknown role.");
                    return true; // DO repeat, server role can change
            }

            // ensure we do not run if not main domain, but do NOT lock it
            if (_runtime.IsMainDom == false)
            {
                _logger.Debug<LogScrubber>("Does not run if not MainDom.");
                return false; // do NOT repeat, going down
            }

            // running on a background task, and Log.CleanLogs uses the old SqlHelper,
            // better wrap in a scope and ensure it's all cleaned up and nothing leaks
            using (var scope = _scopeProvider.CreateScope())
            using (_proflog.DebugDuration<LogScrubber>("Log scrubbing executing", "Log scrubbing complete"))
            {
                _auditService.CleanLogs(GetLogScrubbingMaximumAge(_settings));
                scope.Complete();
            }

            return true; // repeat
        }

        public override bool IsAsync => false;
    }
}
