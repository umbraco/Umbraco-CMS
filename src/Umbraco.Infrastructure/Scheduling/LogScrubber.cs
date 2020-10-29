using System;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;
using Microsoft.Extensions.Logging;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Web.Scheduling
{
    [UmbracoVolatile]
    public class LogScrubber : RecurringTaskBase
    {
        private readonly IMainDom _mainDom;
        private readonly IServerRegistrar _serverRegistrar;
        private readonly IAuditService _auditService;
        private readonly LoggingSettings _settings;
        private readonly IProfilingLogger _profilingLogger;
        private readonly ILogger<LogScrubber> _logger;
        private readonly IScopeProvider _scopeProvider;

        public LogScrubber(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds,
            IMainDom mainDom, IServerRegistrar serverRegistrar, IAuditService auditService, IOptions<LoggingSettings> settings, IScopeProvider scopeProvider, IProfilingLogger profilingLogger , ILogger<LogScrubber> logger)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _mainDom = mainDom;
            _serverRegistrar = serverRegistrar;
            _auditService = auditService;
            _settings = settings.Value;
            _scopeProvider = scopeProvider;
            _profilingLogger = profilingLogger ;
            _logger = logger;
        }

        // maximum age, in minutes
        private int GetLogScrubbingMaximumAge(LoggingSettings settings)
        {
            var maximumAge = 24 * 60; // 24 hours, in minutes
            try
            {
                if (settings.MaxLogAge > -1)
                    maximumAge = settings.MaxLogAge;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to locate a log scrubbing maximum age. Defaulting to 24 hours.");
            }
            return maximumAge;

        }

        public static int GetLogScrubbingInterval()
        {
            const int interval = 4 * 60 * 60 * 1000; // 4 hours, in milliseconds
            return interval;
        }

        public override bool PerformRun()
        {
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

            // Ensure we use an explicit scope since we are running on a background thread.
            using (var scope = _scopeProvider.CreateScope())
            using (_profilingLogger.DebugDuration<LogScrubber>("Log scrubbing executing", "Log scrubbing complete"))
            {
                _auditService.CleanLogs(GetLogScrubbingMaximumAge(_settings));
                scope.Complete();
            }

            return true; // repeat
        }

        public override bool IsAsync => false;
    }
}
