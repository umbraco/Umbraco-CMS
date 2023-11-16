// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
///     Log scrubbing hosted service.
/// </summary>
/// <remarks>
///     Will only run on non-replica servers.
/// </remarks>
public class LogScrubberJob : IRecurringBackgroundJob
{
    public TimeSpan Period { get => TimeSpan.FromHours(4); }

    // No-op event as the period never changes on this job
    public event EventHandler PeriodChanged { add { } remove { } }


    private readonly IAuditService _auditService;
    private readonly ILogger<LogScrubberJob> _logger;
    private readonly IProfilingLogger _profilingLogger;
    private readonly ICoreScopeProvider _scopeProvider;    
    private LoggingSettings _settings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LogScrubberJob" /> class.
    /// </summary>
    /// <param name="auditService">Service for handling audit operations.</param>
    /// <param name="settings">The configuration for logging settings.</param>
    /// <param name="scopeProvider">Provides scopes for database operations.</param>
    /// <param name="logger">The typed logger.</param>
    /// <param name="profilingLogger">The profiling logger.</param>
    public LogScrubberJob(   
        IAuditService auditService,
        IOptionsMonitor<LoggingSettings> settings,
        ICoreScopeProvider scopeProvider,
        ILogger<LogScrubberJob> logger,
        IProfilingLogger profilingLogger)
    {
       
        _auditService = auditService;
        _settings = settings.CurrentValue;
        _scopeProvider = scopeProvider;
        _logger = logger;
        _profilingLogger = profilingLogger;
        settings.OnChange(x => _settings = x);
    }

    public Task RunJobAsync()
    {
        
        // Ensure we use an explicit scope since we are running on a background thread.
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        using (_profilingLogger.DebugDuration<LogScrubberJob>("Log scrubbing executing", "Log scrubbing complete"))
        {
            _auditService.CleanLogs((int)_settings.MaxLogAge.TotalMinutes);
            _ = scope.Complete();
        }

        return Task.CompletedTask;
    }
}
