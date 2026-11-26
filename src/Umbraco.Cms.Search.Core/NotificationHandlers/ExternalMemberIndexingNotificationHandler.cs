using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Search.Core.Cache.ExternalMember;
using Umbraco.Cms.Search.Core.Services.ExternalMemberIndexing;

namespace Umbraco.Cms.Search.Core.NotificationHandlers;

/// <summary>
/// Keeps external members in the member indexes, both when external members change and when a member index is rebuilt.
/// </summary>
internal sealed class ExternalMemberIndexingNotificationHandler : IndexingNotificationHandlerBase,
    INotificationHandler<ExternalMemberIndexCacheRefresherNotification>,
    INotificationAsyncHandler<IndexRebuildingNotification>
{
    private readonly ExternalMemberIndexingService _externalMemberIndexingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalMemberIndexingNotificationHandler"/> class.
    /// </summary>
    /// <param name="coreScopeProvider">The scope provider used to defer indexing until the ambient scope completes.</param>
    /// <param name="externalMemberIndexingService">The service that maintains external members in the member indexes.</param>
    public ExternalMemberIndexingNotificationHandler(ICoreScopeProvider coreScopeProvider, ExternalMemberIndexingService externalMemberIndexingService)
        : base(coreScopeProvider)
        => _externalMemberIndexingService = externalMemberIndexingService;

    /// <inheritdoc />
    public void Handle(ExternalMemberIndexCacheRefresherNotification notification)
    {
        ExternalMemberIndexCacheRefresher.JsonPayload[] payloads = GetNotificationPayloads<ExternalMemberIndexCacheRefresher.JsonPayload>(notification, out var origin);

        Guid[] removedMemberKeys = payloads
            .Where(payload => payload.ChangeTypes is TreeChangeTypes.Remove)
            .Select(payload => payload.MemberKey)
            .ToArray();
        Guid[] memberKeys = payloads
            .Select(payload => payload.MemberKey)
            .Except(removedMemberKeys)
            .ToArray();

        ExecuteDeferred(() => _externalMemberIndexingService.Handle(memberKeys, removedMemberKeys, origin));
    }

    /// <inheritdoc />
    public async Task HandleAsync(IndexRebuildingNotification notification, CancellationToken cancellationToken)
        => await _externalMemberIndexingService.RebuildAsync(notification.IndexAlias, cancellationToken);
}
