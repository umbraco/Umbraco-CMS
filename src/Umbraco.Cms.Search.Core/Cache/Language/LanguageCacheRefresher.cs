using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Search.Core.Cache.Language;

internal sealed class LanguageCacheRefresher : PayloadCacheRefresherBase<LanguageCacheRefresherNotification, ContentCacheRefresherNotificationPayload<LanguageCacheRefresher.JsonPayload>>
{
    public static readonly Guid UniqueId = Guid.Parse("EB0208D6-9EC5-4B88-B2CE-62C0BFF1DB9A");

    public LanguageCacheRefresher(AppCaches appCaches, IJsonSerializer serializer, IEventAggregator eventAggregator, ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
    }

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Language Cache Refresher";

    public record JsonPayload(Guid LanguageKey, string IsoCode, LanguageChangeTypes ChangeTypes)
    {
    }
}
