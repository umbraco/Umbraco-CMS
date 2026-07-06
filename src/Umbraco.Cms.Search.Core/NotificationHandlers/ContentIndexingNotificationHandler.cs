using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Search.Core.Cache.Content;
using Umbraco.Cms.Search.Core.Cache.Media;
using Umbraco.Cms.Search.Core.Cache.Member;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.NotificationHandlers;

internal sealed class ContentIndexingNotificationHandler : IndexingNotificationHandlerBase,
    INotificationHandler<PublishedContentCacheRefresherNotification>,
    INotificationHandler<DraftContentCacheRefresherNotification>,
    INotificationHandler<DraftMediaCacheRefresherNotification>,
    INotificationHandler<DraftMemberCacheRefresherNotification>
{
    private readonly IContentIndexingService _contentIndexingService;

    public ContentIndexingNotificationHandler(
        ICoreScopeProvider coreScopeProvider,
        IContentIndexingService contentIndexingService)
        : base(coreScopeProvider)
        => _contentIndexingService = contentIndexingService;

    public void Handle(PublishedContentCacheRefresherNotification notification)
    {
        PublishedContentCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads<PublishedContentCacheRefresher.JsonPayload>(notification, out var origin);

        ContentChange[] changes = PublishedDocumentChanges(
            payloads.Select(payload => (payload.ContentKey, TreeChangeTypes: payload.ChangeTypes)));

        ExecuteDeferred(() => _contentIndexingService.Handle(changes, origin));
    }

    public void Handle(DraftContentCacheRefresherNotification notification)
    {
        DraftContentCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads<DraftContentCacheRefresher.JsonPayload>(notification, out var origin);

        ContentChange[] changes = DraftDocumentChanges(
            payloads.Select(payload => (payload.ContentKey, payload.ChangeTypes)));

        ExecuteDeferred(() => _contentIndexingService.Handle(changes, origin));
    }

    public void Handle(DraftMediaCacheRefresherNotification notification)
    {
        DraftMediaCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads<DraftMediaCacheRefresher.JsonPayload>(notification, out var origin);

        ContentChange[] changes = MediaChanges(
            payloads.Select(payload => (payload.MediaKey, payload.ChangeTypes)));

        ExecuteDeferred(() => _contentIndexingService.Handle(changes, origin));
    }


    public void Handle(DraftMemberCacheRefresherNotification notification)
    {
        DraftMemberCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads<DraftMemberCacheRefresher.JsonPayload>(notification, out var origin);

        ContentChange[] changes = MemberChanges(
            payloads.Select(payload => (payload.MemberKey, payload.ChangeTypes)));

        ExecuteDeferred(() => _contentIndexingService.Handle(changes, origin));
    }

    private ContentChange[] PublishedDocumentChanges(IEnumerable<(Guid ContentId, TreeChangeTypes ChangeTypes)> payloads)
        => GetContentChanges(
            payloads,
            (contentKey, changeImpact) => ContentChange.Document(contentKey, changeImpact, ContentState.Published));

    private ContentChange[] DraftDocumentChanges(IEnumerable<(Guid ContentId, TreeChangeTypes ChangeTypes)> payloads)
        => GetContentChanges(
            payloads,
            (contentKey, changeImpact) => ContentChange.Document(contentKey, changeImpact, ContentState.Draft));

    private ContentChange[] MediaChanges(IEnumerable<(Guid ContentId, TreeChangeTypes ChangeTypes)> payloads)
        => GetContentChanges(
            payloads,
            (contentKey, changeImpact) => ContentChange.Media(contentKey, changeImpact, ContentState.Draft));

    private ContentChange[] MemberChanges(IEnumerable<(Guid ContentId, TreeChangeTypes ChangeTypes)> payloads)
        => GetContentChanges(
            payloads,
            (contentKey, changeImpact) => ContentChange.Member(contentKey, changeImpact, ContentState.Draft));

    private ContentChange[] GetContentChanges(IEnumerable<(Guid ContentId, TreeChangeTypes ChangeTypes)> payloads, Func<Guid, ChangeImpact, ContentChange> contentChangeFactory)
        => payloads
            .Select(payload => payload.ChangeTypes switch
                {
                    TreeChangeTypes.None => null,
                    TreeChangeTypes.RefreshAll => contentChangeFactory(payload.ContentId, ChangeImpact.RefreshWithDescendants),
                    TreeChangeTypes.RefreshNode => contentChangeFactory(payload.ContentId, ChangeImpact.Refresh),
                    TreeChangeTypes.RefreshBranch => contentChangeFactory(payload.ContentId, ChangeImpact.RefreshWithDescendants),
                    TreeChangeTypes.Remove => contentChangeFactory(payload.ContentId, ChangeImpact.Remove),
                    _ => throw new ArgumentOutOfRangeException(nameof(payload), payload.ChangeTypes, "Unexpected tree change type.")
                })
            .WhereNotNull()
            .ToArray();
}
