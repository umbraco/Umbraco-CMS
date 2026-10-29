using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Search.Core.Cache.ExternalMember;

/// <summary>
/// Reacts to external member changes and broadcasts them via <see cref="ExternalMemberIndexCacheRefresher"/>.
/// </summary>
internal sealed class ExternalMemberNotificationHandler : ContentNotificationHandlerBase<ExternalMemberIndexCacheRefresher.JsonPayload>,
    IDistributedCacheNotificationHandler<ExternalMemberSavedNotification>,
    IDistributedCacheNotificationHandler<ExternalMemberDeletedNotification>
{
    /// <inheritdoc />
    protected override Guid CacheRefresherUniqueId => ExternalMemberIndexCacheRefresher.UniqueId;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalMemberNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache used to broadcast the paired cache refresher notification.</param>
    /// <param name="originProvider">The provider of the current server origin.</param>
    /// <param name="indexDocumentService">The service passed to the base class for change-detection cache handling.</param>
    public ExternalMemberNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService)
        : base(distributedCache, originProvider, indexDocumentService)
    {
    }

    /// <inheritdoc />
    /// <remarks>
    /// Skips re-indexing when the originating save explicitly signalled that no indexable field changed
    /// (e.g. a login-only update), via
    /// <see cref="Umbraco.Cms.Core.Constants.Conventions.Member.IndexableFieldsChangedStateKey"/>.
    /// </remarks>
    public void Handle(ExternalMemberSavedNotification notification)
    {
        if (IndexableFieldsChanged(notification.State) is false)
        {
            return;
        }

        HandlePayloads(notification.SavedEntities, TreeChangeTypes.RefreshNode);
    }

    /// <inheritdoc />
    public void Handle(ExternalMemberDeletedNotification notification)
        => HandlePayloads(notification.DeletedEntities, TreeChangeTypes.Remove);

    private void HandlePayloads(IEnumerable<ExternalMemberIdentity> entities, TreeChangeTypes changeTypes)
    {
        ExternalMemberIndexCacheRefresher.JsonPayload[] payloads = entities
            .Select(entity => new ExternalMemberIndexCacheRefresher.JsonPayload(entity.Key, changeTypes))
            .ToArray();
        if (payloads.Length is 0)
        {
            return;
        }

        HandlePayloads(payloads);
    }

    private static bool IndexableFieldsChanged(IDictionary<string, object?> state)
        => state.TryGetValue(Umbraco.Cms.Core.Constants.Conventions.Member.IndexableFieldsChangedStateKey, out object? value) is false
           || value is not bool flag
           || flag;
}
