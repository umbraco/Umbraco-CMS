using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
public sealed class DictionaryItemDeletedDistributedCacheNotificationHandler : DeletedDistributedCacheNotificationHandlerBase<IDictionaryItem, DictionaryItemDeletedNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryItemDeletedDistributedCacheNotificationHandler" /> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public DictionaryItemDeletedDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    protected override void Handle(IEnumerable<IDictionaryItem> entities, IDictionary<string, object?> state)
        => _distributedCache.RemoveDictionaryCache(entities);
}
