using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.Language;

/// <summary>
/// Reacts to language deletions and broadcasts them via <see cref="LanguageCacheRefresher"/>, flushing the deleted cultures from the change-detection cache.
/// </summary>
internal sealed class LanguageNotificationHandler
    : ContentNotificationHandlerBase<LanguageCacheRefresher.JsonPayload>,
        IDistributedCacheNotificationHandler<LanguageDeletedNotification>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageNotificationHandler"/> class.
    /// </summary>
    public LanguageNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService)
        : base(distributedCache, originProvider, indexDocumentService)
    {
    }

    /// <inheritdoc />
    protected override Guid CacheRefresherUniqueId => LanguageCacheRefresher.UniqueId;

    /// <inheritdoc />
    public void Handle(LanguageDeletedNotification notification)
    {
        ILanguage[] deletedEntities = notification.DeletedEntities.ToArray();
        if (deletedEntities.Length is 0)
        {
            return;
        }

        var isoCodes = deletedEntities.Select(language => language.IsoCode).ToArray();
        RemoveLanguageFromDocumentIndexCache(isoCodes);

        LanguageCacheRefresher.JsonPayload[] payloads = deletedEntities
            .Select(language => new LanguageCacheRefresher.JsonPayload(language.Key, language.IsoCode, LanguageChangeTypes.Delete))
            .ToArray();

        HandlePayloads(payloads);
    }
}
