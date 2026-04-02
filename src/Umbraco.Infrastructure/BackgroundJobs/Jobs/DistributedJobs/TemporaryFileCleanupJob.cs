using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.DistributedJobs;

/// <summary>
/// Cleans up temporary media files.
/// </summary>
internal class TemporaryFileCleanupJob : IDistributedBackgroundJob
{
    /// <inheritdoc />
    public string Name => "TemporaryFileCleanupJob";

    /// <inheritdoc />
    public TimeSpan Period => TimeSpan.FromMinutes(5);

    private readonly ILogger<TemporaryFileCleanupJob> _logger;
    private readonly ITemporaryFileService _service;


    /// <summary>
    ///     Initializes a new instance of the <see cref="TemporaryFileCleanupJob" /> class.
    /// </summary>
    /// <param name="logger">The logger used to record job execution details and errors.</param>
    /// <param name="temporaryFileService">The service responsible for managing temporary files.</param>
    public TemporaryFileCleanupJob(
        ILogger<TemporaryFileCleanupJob> logger,
        ITemporaryFileService temporaryFileService)
    {
        _logger = logger;
        _service = temporaryFileService;
    }


    /// <inheritdoc />
    public  async Task ExecuteAsync()
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
