using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Sync;

namespace Umbraco.Infrastructure.HostedServices
{
    /// <summary>
    /// Log scrubbing hosted service.
    /// </summary>
    /// <remarks>
    /// Will only run on non-replica servers.
    /// </remarks>
    public class LogScrubber : RecurringHostedServiceBase
    {
        private readonly IMainDom _mainDom;
        private readonly IServerRegistrar _serverRegistrar;
        private readonly IAuditService _auditService;
        private readonly LoggingSettings _settings;
        private readonly IProfilingLogger _profilingLogger;
        private readonly ILogger<LogScrubber> _logger;
        private readonly IScopeProvider _scopeProvider;

        public LogScrubber(IMainDom mainDom, IServerRegistrar serverRegistrar, IAuditService auditService, IOptions<LoggingSettings> settings, IScopeProvider scopeProvider, ILogger<LogScrubber> logger, IProfilingLogger profilingLogger)
            : base(TimeSpan.FromHours(4), DefaultDelay)
        {
            _mainDom = mainDom;
            _serverRegistrar = serverRegistrar;
            _auditService = auditService;
            _settings = settings.Value;
            _scopeProvider = scopeProvider;
            _logger = logger;
            _profilingLogger = profilingLogger;
        }

        internal override async Task PerformExecuteAsync(object state)
        {
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

            // Ensure we use an explicit scope since we are running on a background thread.
            using (var scope = _scopeProvider.CreateScope())
            using (_profilingLogger.DebugDuration<LogScrubber>("Log scrubbing executing", "Log scrubbing complete"))
            {
                _auditService.CleanLogs((int)_settings.MaxLogAge.TotalMinutes);
                scope.Complete();
            }
        }
    }
}
