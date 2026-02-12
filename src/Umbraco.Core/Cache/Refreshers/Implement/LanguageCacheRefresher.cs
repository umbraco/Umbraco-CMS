using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;
using static Umbraco.Cms.Core.Cache.LanguageCacheRefresher.JsonPayload;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
///     Cache refresher for language caches.
/// </summary>
public sealed class LanguageCacheRefresher : PayloadCacheRefresherBase<LanguageCacheRefresherNotification,
    LanguageCacheRefresher.JsonPayload>
{
    private readonly IDomainCacheService _domainCacheService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LanguageCacheRefresher" /> class.
    /// </summary>
    /// <param name="appCaches">The application caches.</param>
    /// <param name="serializer">The JSON serializer.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="domainCache">The domain cache service.</param>
    /// <param name="factory">The cache refresher notification factory.</param>
    public LanguageCacheRefresher(
        AppCaches appCaches,
        IJsonSerializer serializer,
        IEventAggregator eventAggregator,
        IDomainCacheService domainCache,
        ICacheRefresherNotificationFactory factory)
        : base(appCaches, serializer, eventAggregator, factory)
    {
        _domainCacheService = domainCache;
    }

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
        _domainCacheService.Refresh(payloads);
    }

    #region Json

    /// <summary>
    ///     Represents the JSON payload for language cache refresh operations.
    /// </summary>
    public class JsonPayload
    {
        /// <summary>
        ///     Defines the types of changes that can occur to a language.
        /// </summary>
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonPayload" /> class.
        /// </summary>
        /// <param name="id">The identifier of the language.</param>
        /// <param name="isoCode">The ISO code of the language.</param>
        /// <param name="changeType">The type of change that occurred.</param>
        public JsonPayload(int id, string isoCode, LanguageChangeType changeType)
        {
            Id = id;
            IsoCode = isoCode;
            ChangeType = changeType;
        }

        /// <summary>
        ///     Gets the identifier of the language.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Gets the ISO code of the language.
        /// </summary>
        public string IsoCode { get; }

        /// <summary>
        ///     Gets the type of change that occurred.
        /// </summary>
        public LanguageChangeType ChangeType { get; }
    }

    #endregion

    #region Define

    /// <summary>
    ///     The unique identifier for this cache refresher.
    /// </summary>
    public static readonly Guid UniqueId = Guid.Parse("3E0F95D8-0BE5-44B8-8394-2B8750B62654");

    /// <inheritdoc />
    public override Guid RefresherUniqueId => UniqueId;

    /// <inheritdoc />
    public override string Name => "Language Cache Refresher";

    #endregion

    #region Refresher

    /// <inheritdoc />
    public override void RefreshInternal(JsonPayload[] payloads)
    {
        // Languages has no concept of "published" languages, so all caches are "internal"
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
        }
    }


    // these events should never trigger
    // everything should be PAYLOAD/JSON

    /// <inheritdoc />
    public override void RefreshAll() => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Refresh(int id) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Refresh(Guid id) => throw new NotSupportedException();

    /// <inheritdoc />
    public override void Remove(int id) => throw new NotSupportedException();

    #endregion
}
