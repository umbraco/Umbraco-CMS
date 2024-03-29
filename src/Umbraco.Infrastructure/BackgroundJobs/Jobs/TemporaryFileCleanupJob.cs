using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs;

public class TemporaryFileCleanupJob : IRecurringBackgroundJob
{
    public TimeSpan Period { get => TimeSpan.FromMinutes(5); }
    public TimeSpan Delay { get => TimeSpan.FromMinutes(5); }

    // No-op event as the period never changes on this job
    public event EventHandler PeriodChanged { add { } remove { } }

    private readonly ILogger<TemporaryFileCleanupJob> _logger;
    private readonly ITemporaryFileService _service;


    /// <summary>
    ///     Initializes a new instance of the <see cref="TemporaryFileCleanupJob" /> class.
    /// </summary>
    public TemporaryFileCleanupJob(
        ILogger<TemporaryFileCleanupJob> logger,
        ITemporaryFileService temporaryFileService)
    {
        _logger = logger;
        _service = temporaryFileService;
    }

    /// <summary>
    /// Runs the background task to send the anonymous ID
    /// to telemetry service
    /// </summary>
    public  async Task RunJobAsync()
    {
        var count = (await _service.CleanUpOldTempFiles()).Count();

        if (count > 0)
        {
            _logger.LogDebug("Deleted {Count} temporary file(s)", count);
        }
        else
        {
            _logger.LogDebug("Task complete, no items were deleted");
        }
    }
}
