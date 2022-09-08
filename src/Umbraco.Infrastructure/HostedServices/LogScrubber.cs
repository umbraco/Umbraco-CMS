// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.HostedServices;

/// <summary>
///     Log scrubbing hosted service.
/// </summary>
/// <remarks>
///     Will only run on non-replica servers.
/// </remarks>
public class LogScrubber : RecurringHostedServiceBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<LogScrubber> _logger;
    private readonly IMainDom _mainDom;
    private readonly IProfilingLogger _profilingLogger;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IServerRoleAccessor _serverRegistrar;
    private LoggingSettings _settings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LogScrubber" /> class.
    /// </summary>
    /// <param name="mainDom">Representation of the main application domain.</param>
    /// <param name="serverRegistrar">Provider of server registrations to the distributed cache.</param>
    /// <param name="auditService">Service for handling audit operations.</param>
    /// <param name="settings">The configuration for logging settings.</param>
    /// <param name="scopeProvider">Provides scopes for database operations.</param>
    /// <param name="logger">The typed logger.</param>
    /// <param name="profilingLogger">The profiling logger.</param>
    public LogScrubber(
        IMainDom mainDom,
        IServerRoleAccessor serverRegistrar,
        IAuditService auditService,
        IOptionsMonitor<LoggingSettings> settings,
        ICoreScopeProvider scopeProvider,
        ILogger<LogScrubber> logger,
        IProfilingLogger profilingLogger)
        : base(logger, TimeSpan.FromHours(4), DefaultDelay)
    {
        _mainDom = mainDom;
        _serverRegistrar = serverRegistrar;
        _auditService = auditService;
        _settings = settings.CurrentValue;
        _scopeProvider = scopeProvider;
        _logger = logger;
        _profilingLogger = profilingLogger;
        settings.OnChange(x => _settings = x);
    }

    public override Task PerformExecuteAsync(object? state)
    {
        switch (_serverRegistrar.CurrentServerRole)
        {
            case ServerRole.Subscriber:
                _logger.LogDebug("Does not run on subscriber servers.");
                return Task.CompletedTask;
            case ServerRole.Unknown:
                _logger.LogDebug("Does not run on servers with unknown role.");
                return Task.CompletedTask;
        }

        // Ensure we do not run if not main domain, but do NOT lock it
        if (_mainDom.IsMainDom == false)
        {
            _logger.LogDebug("Does not run if not MainDom.");
            return Task.CompletedTask;
        }

        // Ensure we use an explicit scope since we are running on a background thread.
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        using (_profilingLogger.DebugDuration<LogScrubber>("Log scrubbing executing", "Log scrubbing complete"))
        {
            _auditService.CleanLogs((int)_settings.MaxLogAge.TotalMinutes);
            _ = scope.Complete();
        }

        return Task.CompletedTask;
    }
}
