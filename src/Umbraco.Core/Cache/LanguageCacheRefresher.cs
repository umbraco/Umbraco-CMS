using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;
using static Umbraco.Cms.Core.Cache.LanguageCacheRefresher.JsonPayload;

namespace Umbraco.Cms.Core.Cache;

public sealed class LanguageCacheRefresher : PayloadCacheRefresherBase<LanguageCacheRefresherNotification,
    LanguageCacheRefresher.JsonPayload>
{
    public LanguageCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IPublishedSnapshotService publishedSnapshotService,
        IEventAggregator eventAggregator,
        ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory) =>
        _publishedSnapshotService = publishedSnapshotService;

    /// <summary>
    ///     Clears all domain caches
    /// </summary>
    private void RefreshDomains()
    {
        ClearAllIsolatedCacheByEntityType<IDomain>();

        // note: must do what's above FIRST else the repositories still have the old cached
        // content and when the PublishedCachesService is notified of changes it does not see
        // the new content...
        DomainCacheRefresher.JsonPayload[] payloads = new[]
        {
            new DomainCacheRefresher.JsonPayload(0, DomainChangeTypes.RefreshAll),
        };
        _publishedSnapshotService.Notify(payloads);
    }

    #region Json

    public class JsonPayload
    {
        public enum LanguageChangeType
        {
            /// <summary>
            ///     A new languages has been added
            /// </summary>
            Add = 0,

            /// <summary>
            ///     A language has been deleted
            /// </summary>
            Remove = 1,

            /// <summary>
            ///     A language has been updated - but it's culture remains the same
            /// </summary>
            Update = 2,

            /// <summary>
            ///     A language has been updated - it's culture has changed
            /// </summary>
            ChangeCulture = 3,
        }

        public JsonPayload(int id, string isoCode, LanguageChangeType changeType)
        {
            Id = id;
            IsoCode = isoCode;
            ChangeType = changeType;
        }

        public int Id { get; }

        public string IsoCode { get; }

        public LanguageChangeType ChangeType { get; }
    }

    #endregion

    #region Define

    public static readonly Guid UniqueId = Guid.Parse("3E0F95D8-0BE5-44B8-8394-2B8750B62654");
    private readonly IPublishedSnapshotService _publishedSnapshotService;

    public override Guid RefresherUniqueId => UniqueId;

    public override string Name => "Language Cache Refresher";

    #endregion

    #region Refresher

    public override void Refresh(JsonPayload[] payloads)
    {
        if (payloads.Length == 0)
        {
            return;
        }

        var clearDictionary = false;
        var clearContent = false;

        // clear all no matter what type of payload
        ClearAllIsolatedCacheByEntityType<ILanguage>();

        foreach (JsonPayload payload in payloads)
        {
            switch (payload.ChangeType)
            {
                case LanguageChangeType.Update:
                    clearDictionary = true;
                    break;
                case LanguageChangeType.Remove:
                case LanguageChangeType.ChangeCulture:
                    clearDictionary = true;
                    clearContent = true;
                    break;
            }
        }

        if (clearDictionary)
        {
            ClearAllIsolatedCacheByEntityType<IDictionaryItem>();
        }

        // if this flag is set, we will tell the published snapshot service to refresh ALL content and evict ALL IContent items
        if (clearContent)
        {
            // clear all domain caches
            RefreshDomains();
            ContentCacheRefresher.RefreshContentTypes(AppCaches); // we need to evict all IContent items

            // now refresh all nucache
            ContentCacheRefresher.JsonPayload[] clearContentPayload =
                new[] { new ContentCacheRefresher.JsonPayload(0, null, TreeChangeTypes.RefreshAll) };
            ContentCacheRefresher.NotifyPublishedSnapshotService(_publishedSnapshotService, AppCaches, clearContentPayload);
        }

        // then trigger event
        base.Refresh(payloads);
    }

    // these events should never trigger
    // everything should be PAYLOAD/JSON
    public override void RefreshAll() => throw new NotSupportedException();

    public override void Refresh(int id) => throw new NotSupportedException();

    public override void Refresh(Guid id) => throw new NotSupportedException();

    public override void Remove(int id) => throw new NotSupportedException();

    #endregion
}
