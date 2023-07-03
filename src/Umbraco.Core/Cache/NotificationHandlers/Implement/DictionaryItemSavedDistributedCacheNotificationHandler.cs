using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
public sealed class DictionaryItemSavedDistributedCacheNotificationHandler : SavedDistributedCacheNotificationHandlerBase<IDictionaryItem, DictionaryItemSavedNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryItemSavedDistributedCacheNotificationHandler" /> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public DictionaryItemSavedDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    protected override void Handle(IEnumerable<IDictionaryItem> entities)
        => _distributedCache.RefreshDictionaryCache(entities);
}
