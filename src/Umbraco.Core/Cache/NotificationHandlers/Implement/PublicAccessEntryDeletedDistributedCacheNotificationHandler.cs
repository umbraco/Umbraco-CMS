using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
public sealed class PublicAccessEntryDeletedDistributedCacheNotificationHandler : DeletedDistributedCacheNotificationHandlerBase<PublicAccessEntry, PublicAccessEntryDeletedNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicAccessEntryDeletedDistributedCacheNotificationHandler" /> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public PublicAccessEntryDeletedDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    [Obsolete("Scheduled for removal in Umbraco 18.")]
    protected override void Handle(IEnumerable<PublicAccessEntry> entities)
        => Handle(entities, new Dictionary<string, object?>());

    /// <inheritdoc />
    protected override void Handle(IEnumerable<PublicAccessEntry> entities, IDictionary<string, object?> state)
        => _distributedCache.RefreshPublicAccess();
}
