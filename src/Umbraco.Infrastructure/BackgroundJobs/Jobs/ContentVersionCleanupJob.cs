using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

/// <summary>
///     Recurring hosted service that executes the content history cleanup.
/// </summary>
public class ContentVersionCleanupJob : IRecurringBackgroundJob
{

    public TimeSpan Period { get => TimeSpan.FromHours(1); }

    // No-op event as the period never changes on this job
    public event EventHandler PeriodChanged { add { } remove { } }

    private readonly ILogger<ContentVersionCleanupJob> _logger;
    private readonly IContentVersionService _service;
    private readonly IOptionsMonitor<ContentSettings> _settingsMonitor;


    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentVersionCleanupJob" /> class.
    /// </summary>
    public ContentVersionCleanupJob(
        ILogger<ContentVersionCleanupJob> logger,
        IOptionsMonitor<ContentSettings> settingsMonitor,
        IContentVersionService service)
    {
        _logger = logger;
        _settingsMonitor = settingsMonitor;
        _service = service;
    }

    /// <inheritdoc />
    public Task RunJobAsync()
    {
        // Globally disabled by feature flag
        if (!_settingsMonitor.CurrentValue.ContentVersionCleanupPolicy.EnableCleanup)
        {
            _logger.LogInformation(
                "ContentVersionCleanup task will not run as it has been globally disabled via configuration");
            return Task.CompletedTask;
        }


        var count = _service.PerformContentVersionCleanup(DateTime.Now).Count;

        if (count > 0)
        {
            _logger.LogInformation("Deleted {count} ContentVersion(s)", count);
        }
        else
        {
            _logger.LogDebug("Task complete, no items were Deleted");
        }

        return Task.CompletedTask;
    }
}
