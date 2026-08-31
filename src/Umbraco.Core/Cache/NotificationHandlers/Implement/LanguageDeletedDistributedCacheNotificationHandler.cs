using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
public sealed class LanguageDeletedDistributedCacheNotificationHandler : DeletedDistributedCacheNotificationHandlerBase<ILanguage, LanguageDeletedNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageDeletedDistributedCacheNotificationHandler" /> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public LanguageDeletedDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    protected override void Handle(IEnumerable<ILanguage> entities, IDictionary<string, object?> state)
    {
        _distributedCache.RemoveLanguageCache(entities);

        // User groups cache their allowed language ids, so a deleted language must be evicted from
        // them too - otherwise a stale, now-missing id lingers on the cached user group and breaks
        // reads that resolve those ids. This is a deliberately coarse refresh of the entire user group
        // and user caches (RefreshAll also clears IUser): we can't know which groups reference the
        // language without a query, and language deletion is rare enough that a full refresh is fine.
        _distributedCache.RefreshAllUserGroupCache();
    }
}
