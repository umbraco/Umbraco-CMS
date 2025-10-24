using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Install;

/// <summary>
/// Handles notifications indicating that migration plans have been executed, and triggers a refresh of the distributed
/// published content cache as needed.
/// </summary>
internal class MigrationPlansExecutedNotificationHandler : INotificationHandler<MigrationPlansExecutedNotification>
{
    private readonly DistributedCache _distributedCache;
    private readonly ILogger<MigrationPlansExecutedNotificationHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationPlansExecutedNotificationHandler"/> class.
    /// </summary>
    public MigrationPlansExecutedNotificationHandler(DistributedCache distributedCache, ILogger<MigrationPlansExecutedNotificationHandler> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Handle(MigrationPlansExecutedNotification notification)
    {
        if (notification.ExecutedPlans.Count == 0)
        {
            return;
        }

        // One or more migrations were executed, which may have published content, so we need to reload the memory cache.
        _distributedCache.RefreshAllPublishedSnapshot();
        _logger.LogInformation(
            "Migration plans executed: {Plans}. Triggered refresh of distributed published content cache.",
            string.Join(", ", notification.ExecutedPlans.Select(x => x.Plan.Name)));
    }
}
