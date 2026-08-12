using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Search.Core.Cache.ContentType;
using Umbraco.Cms.Search.Core.Cache.Index;
using Umbraco.Cms.Search.Core.Cache.Language;
using Umbraco.Cms.Search.Core.Cache.MediaType;
using Umbraco.Cms.Search.Core.Cache.MemberType;
using Umbraco.Cms.Search.Core.Configuration;
using Umbraco.Cms.Search.Core.Models.Configuration;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.NotificationHandlers;

/// <summary>
/// Triggers full or partial index rebuilds in response to language, content/media/member type changes, and explicit rebuild requests.
/// </summary>
internal sealed class RebuildIndexesNotificationHandler : IndexingNotificationHandlerBase,
    INotificationHandler<LanguageCacheRefresherNotification>,
    INotificationHandler<ContentTypeCacheRefresherNotification>,
    INotificationHandler<MemberTypeCacheRefresherNotification>,
    INotificationHandler<MediaTypeCacheRefresherNotification>,
    INotificationHandler<RebuildIndexCacheRefresherNotification>
{
    private readonly IContentIndexingService _contentIndexingService;
    private readonly IContentTypeIndexingService _contentTypeIndexingService;
    private readonly IndexOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RebuildIndexesNotificationHandler"/> class.
    /// </summary>
    /// <param name="contentIndexingService">The service used to trigger full index rebuilds.</param>
    /// <param name="contentTypeIndexingService">The service used to re-index content affected by a content/media/member type change.</param>
    /// <param name="options">The options describing the registered content indexes.</param>
    /// <param name="coreScopeProvider">The scope provider used to defer actions until the ambient scope completes.</param>
    public RebuildIndexesNotificationHandler(
        IContentIndexingService contentIndexingService,
        IContentTypeIndexingService contentTypeIndexingService,
        IOptions<IndexOptions> options,
        ICoreScopeProvider coreScopeProvider)
        : base(coreScopeProvider)
    {
        _contentIndexingService = contentIndexingService;
        _contentTypeIndexingService = contentTypeIndexingService;
        _options = options.Value;
    }

    /// <summary>
    /// Rebuilds every document index when a language is deleted.
    /// </summary>
    /// <param name="notification">The notification describing the language change to react to.</param>
    public void Handle(LanguageCacheRefresherNotification notification)
    {
        LanguageCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads<LanguageCacheRefresher.JsonPayload>(notification, out var origin);
        if (payloads.Any(payload => payload.ChangeTypes is LanguageChangeTypes.Delete) is false)
        {
            return;
        }

        foreach (ContentIndexRegistration indexRegistration in _options.GetContentIndexRegistrations())
        {
            if (indexRegistration.ContainedObjectTypes.Contains(UmbracoObjectTypes.Document))
            {
                _contentIndexingService.Rebuild(indexRegistration.IndexAlias, origin);
            }
        }
    }

    /// <summary>
    /// Re-indexes content whose content type structure changed.
    /// </summary>
    /// <param name="notification">The notification describing the content type change to react to.</param>
    public void Handle(ContentTypeCacheRefresherNotification notification)
    {
        ContentTypeCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads<ContentTypeCacheRefresher.JsonPayload>(notification, out var origin);

        HandleContentTypeChanges(payloads.Select(payload => (payload.ContentTypeKey, payload.ChangeTypes)), UmbracoObjectTypes.Document, origin);
    }

    /// <summary>
    /// Re-indexes members whose member type structure changed.
    /// </summary>
    /// <param name="notification">The notification describing the member type change to react to.</param>
    public void Handle(MemberTypeCacheRefresherNotification notification)
    {
        MemberTypeCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads<MemberTypeCacheRefresher.JsonPayload>(notification, out var origin);

        HandleContentTypeChanges(payloads.Select(payload => (payload.MemberTypeKey, payload.ChangeTypes)), UmbracoObjectTypes.Member, origin);
    }

    /// <summary>
    /// Re-indexes media whose media type structure changed.
    /// </summary>
    /// <param name="notification">The notification describing the media type change to react to.</param>
    public void Handle(MediaTypeCacheRefresherNotification notification)
    {
        MediaTypeCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads<MediaTypeCacheRefresher.JsonPayload>(notification, out var origin);

        HandleContentTypeChanges(payloads.Select(payload => (payload.MediaTypeKey, payload.ChangeTypes)), UmbracoObjectTypes.Media, origin);
    }

    /// <summary>
    /// Rebuilds the index(es) named in an explicit rebuild request.
    /// </summary>
    /// <param name="notification">The notification describing the rebuild request to react to.</param>
    public void Handle(RebuildIndexCacheRefresherNotification notification)
    {
        RebuildIndexCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads<RebuildIndexCacheRefresher.JsonPayload>(notification, out var origin);

        foreach (RebuildIndexCacheRefresher.JsonPayload payload in payloads)
        {
            _contentIndexingService.Rebuild(payload.IndexAlias, origin);
        }
    }

    private void HandleContentTypeChanges(IEnumerable<(Guid ContentTypeKey, ContentTypeChangeTypes ChangeTypes)> changes, UmbracoObjectTypes objectType, string origin)
    {
        Guid[] affectedContentTypeKeys = changes
            .Where(change => change.ChangeTypes.HasFlag(ContentTypeChangeTypes.RefreshMain) || change.ChangeTypes.HasFlag(ContentTypeChangeTypes.Remove))
            .Select(change => change.ContentTypeKey)
            .Distinct()
            .ToArray();

        if (affectedContentTypeKeys.Length == 0)
        {
            return;
        }

        _contentTypeIndexingService.ReindexByContentTypes(affectedContentTypeKeys, objectType, origin);
    }
}
