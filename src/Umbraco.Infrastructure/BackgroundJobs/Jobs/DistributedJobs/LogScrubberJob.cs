// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.DistributedJobs;

/// <summary>
///     Log scrubbing hosted service.
/// </summary>
/// <remarks>
///     Will only run on non-replica servers.
/// </remarks>
internal class LogScrubberJob : IDistributedBackgroundJob
{
    private readonly IAuditService _auditService;
    private readonly IProfilingLogger _profilingLogger;
    private readonly ICoreScopeProvider _scopeProvider;
    private LoggingSettings _settings;

    /// <inheritdoc />
    public string Name => "LogScrubberJob";

    /// <inheritdoc />
    public TimeSpan Period => TimeSpan.FromHours(4);

    /// <summary>
    ///     Initializes a new instance of the <see cref="LogScrubberJob" /> class.
    /// </summary>
    /// <param name="auditService">Service for handling audit operations.</param>
    /// <param name="settings">The configuration for logging settings.</param>
    /// <param name="scopeProvider">Provides scopes for database operations.</param>
    /// <param name="profilingLogger">The profiling logger.</param>
    public LogScrubberJob(
        IAuditService auditService,
        IOptionsMonitor<LoggingSettings> settings,
        ICoreScopeProvider scopeProvider,
        IProfilingLogger profilingLogger)
    {
        _auditService = auditService;
        _settings = settings.CurrentValue;
        _scopeProvider = scopeProvider;
        _profilingLogger = profilingLogger;
        settings.OnChange(x => _settings = x);
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync()
    {
        // Ensure we use an explicit scope since we are running on a background thread.
        using ICoreScope scope = _scopeProvider.CreateCoreScope();
        using (_profilingLogger.DebugDuration<LogScrubberJob>("Log scrubbing executing", "Log scrubbing complete"))
        {
            await _auditService.CleanLogsAsync((int)_settings.MaxLogAge.TotalMinutes);
            _ = scope.Complete();
        }
    }
}
