using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Search.Core.Cache.Language;

/// <summary>
/// Distributed cache refresher that broadcasts language deletions to other servers, for reindexing affected content.
/// </summary>
internal sealed class LanguageCacheRefresher : PayloadCacheRefresherBase<LanguageCacheRefresherNotification, ContentCacheRefresherNotificationPayload<LanguageCacheRefresher.JsonPayload>>
{
    /// <summary>
    /// The unique identifier of this refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("EB0208D6-9EC5-4B88-B2CE-62C0BFF1DB9A");

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageCacheRefresher"/> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="factory">The notification factory.</param>
    public LanguageCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Language Cache Refresher";

    /// <summary>
    /// The payload broadcast for a single changed language.
    /// </summary>
    /// <param name="LanguageKey">The key of the language.</param>
    /// <param name="IsoCode">The ISO code of the language.</param>
    /// <param name="ChangeTypes">The kind of change that occurred.</param>
    public record JsonPayload(Guid LanguageKey, string IsoCode, LanguageChangeTypes ChangeTypes)
    {
    }
}
