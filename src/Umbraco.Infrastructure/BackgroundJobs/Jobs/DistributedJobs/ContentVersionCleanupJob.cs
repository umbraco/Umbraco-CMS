using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.DistributedJobs;

/// <summary>
///     Recurring hosted service that executes the content history cleanup.
/// </summary>
internal class ContentVersionCleanupJob : IDistributedBackgroundJob
{
    /// <inheritdoc />
    public string Name => "ContentVersionCleanupJob";

    /// <inheritdoc />
    public TimeSpan Period { get => TimeSpan.FromHours(1); }


    private readonly ILogger<ContentVersionCleanupJob> _logger;
    private readonly IContentVersionService _contentVersionService;
    private readonly IElementVersionService _elementVersionService;
    private readonly IOptionsMonitor<ContentSettings> _settingsMonitor;


    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentVersionCleanupJob" /> class.
    /// </summary>
    public ContentVersionCleanupJob(
        ILogger<ContentVersionCleanupJob> logger,
        IOptionsMonitor<ContentSettings> settingsMonitor,
        IContentVersionService contentVersionService,
        IElementVersionService elementVersionService)
    {
        _logger = logger;
        _settingsMonitor = settingsMonitor;
        _contentVersionService = contentVersionService;
        _elementVersionService = elementVersionService;
    }

    /// <inheritdoc />
    public Task ExecuteAsync()
    {
        // Globally disabled by feature flag
        if (!_settingsMonitor.CurrentValue.ContentVersionCleanupPolicy.EnableCleanup)
        {
            _logger.LogInformation(
                "ContentVersionCleanup task will not run as it has been globally disabled via configuration");
            return Task.CompletedTask;
        }

        DateTime asAtDate = DateTime.UtcNow;

        CleanupVersions(Constants.UdiEntityType.Document, () => _contentVersionService.PerformContentVersionCleanup(asAtDate));
        CleanupVersions(Constants.UdiEntityType.Element, () => _elementVersionService.PerformContentVersionCleanup(asAtDate));

        return Task.CompletedTask;
    }

    private void CleanupVersions(string entityType, Func<IReadOnlyCollection<ContentVersionMeta>> cleanup)
    {
        var deletedVersions = cleanup();

        if (deletedVersions.Count > 0)
        {
            _logger.LogInformation("Deleted {Count} {EntityType} version(s)", deletedVersions.Count, entityType);
        }
        else
        {
            _logger.LogDebug("Cleanup complete for {EntityType} versions, no items were deleted", entityType);
        }
    }
}
