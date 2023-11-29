using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
public sealed class PublicAccessEntrySavedDistributedCacheNotificationHandler : SavedDistributedCacheNotificationHandlerBase<PublicAccessEntry, PublicAccessEntrySavedNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicAccessEntrySavedDistributedCacheNotificationHandler" /> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public PublicAccessEntrySavedDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    protected override void Handle(IEnumerable<PublicAccessEntry> entities)
        => _distributedCache.RefreshPublicAccess();
}
