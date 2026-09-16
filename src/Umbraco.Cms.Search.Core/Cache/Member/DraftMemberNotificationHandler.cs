using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Cache.Member;

/// <summary>
/// Reacts to member changes and broadcasts them via <see cref="DraftMemberCacheRefresher"/>, flushing the affected index documents from the change-detection cache.
/// </summary>
internal sealed class DraftMemberNotificationHandler : ContentNotificationHandlerBase<DraftMemberCacheRefresher.JsonPayload>,
    IDistributedCacheNotificationHandler<MemberSavedNotification>,
    IDistributedCacheNotificationHandler<MemberDeletedNotification>
{
    /// <inheritdoc />
    protected override Guid CacheRefresherUniqueId => DraftMemberCacheRefresher.UniqueId;

    /// <summary>
    /// Initializes a new instance of the <see cref="DraftMemberNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache used to broadcast the paired cache refresher notification.</param>
    /// <param name="originProvider">The provider of the current server origin.</param>
    /// <param name="indexDocumentService">The service used to flush the change-detection cache for affected documents.</param>
    public DraftMemberNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService)
        : base(distributedCache, originProvider, indexDocumentService)
    {
    }

    /// <summary>
    /// Flushes the change-detection cache for the given members and broadcasts a refresh-node change for each.
    /// </summary>
    /// <param name="entities">The members to refresh.</param>
    public void Refresh(IEnumerable<IMember> entities)
    {
        IMember[] entitiesAsArray = entities as IMember[] ?? entities.ToArray();
        if (entitiesAsArray.Length is 0)
        {
            return;
        }

        FlushDocumentIndexCache(entitiesAsArray);

        DraftMemberCacheRefresher.JsonPayload[] payloads = entitiesAsArray
            .Select(entity => new DraftMemberCacheRefresher.JsonPayload(entity.Key, TreeChangeTypes.RefreshNode))
            .ToArray();

        HandlePayloads(payloads);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Skips re-indexing when the originating save explicitly signalled that no indexable field changed
    /// (e.g. a login-only update that only touches <c>LastLoginDate</c>/<c>SecurityStamp</c>), via
    /// <see cref="Umbraco.Cms.Core.Constants.Conventions.Member.IndexableFieldsChangedStateKey"/>.
    /// </remarks>
    public void Handle(MemberSavedNotification notification)
    {
        if (IndexableFieldsChanged(notification.State) is false)
        {
            return;
        }

        Refresh(notification.SavedEntities);
    }

    /// <inheritdoc />
    public void Handle(MemberDeletedNotification notification)
    {
        IMember[] deletedEntities = notification.DeletedEntities.ToArray();
        if (deletedEntities.Length is 0)
        {
            return;
        }

        FlushDocumentIndexCache(deletedEntities);

        DraftMemberCacheRefresher.JsonPayload[] payloads = deletedEntities
            .Select(entity => new DraftMemberCacheRefresher.JsonPayload(entity.Key, TreeChangeTypes.Remove))
            .ToArray();

        HandlePayloads(payloads);
    }

    private void FlushDocumentIndexCache(IEnumerable<IMember> entities)
        => FlushDocumentIndexCache(entities.Select(x => x.Key).ToArray(), false);

    // Default to true for backward compatibility — any save that doesn't explicitly signal
    // "nothing indexable changed" is treated as potentially indexable.
    private static bool IndexableFieldsChanged(IDictionary<string, object?> state)
    {
        if (state.TryGetValue(Umbraco.Cms.Core.Constants.Conventions.Member.IndexableFieldsChangedStateKey, out object? value)
            && value is bool flag)
        {
            return flag;
        }

        return true;
    }
}
