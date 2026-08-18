using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.Core.Cache.PublicAccess;

/// <summary>
/// Reacts to public access entry changes and broadcasts them via <see cref="PublicAccessDetailedCacheRefresher"/>, one payload per affected protected content node.
/// </summary>
internal sealed class PublicAccessNotificationHandler : ContentNotificationHandlerBase<PublicAccessDetailedCacheRefresher.JsonPayload>,
    IDistributedCacheNotificationHandler<PublicAccessEntrySavedNotification>,
    IDistributedCacheNotificationHandler<PublicAccessEntryDeletedNotification>
{
    private readonly IIdKeyMap _idKeyMap;

    /// <inheritdoc />
    protected override Guid CacheRefresherUniqueId => PublicAccessDetailedCacheRefresher.UniqueId;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicAccessNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache used to broadcast the paired cache refresher notification.</param>
    /// <param name="originProvider">The provider of the current server origin.</param>
    /// <param name="indexDocumentService">The service used to flush the change-detection cache for affected documents.</param>
    /// <param name="idKeyMap">The map used to resolve the protected content node's key from its numeric ID.</param>
    public PublicAccessNotificationHandler(
        DistributedCache distributedCache,
        IOriginProvider originProvider,
        IIndexDocumentService indexDocumentService,
        IIdKeyMap idKeyMap)
        : base(distributedCache, originProvider, indexDocumentService)
        => _idKeyMap = idKeyMap;

    /// <inheritdoc />
    public void Handle(PublicAccessEntrySavedNotification notification)
        => Handle(notification.SavedEntities);

    /// <inheritdoc />
    public void Handle(PublicAccessEntryDeletedNotification notification)
        => Handle(notification.DeletedEntities);

    private void Handle(IEnumerable<PublicAccessEntry> entities)
    {
        PublicAccessDetailedCacheRefresher.JsonPayload[] payloads = entities.Select(entity =>
            {
                Attempt<Guid> attempt = _idKeyMap.GetKeyForId(entity.ProtectedNodeId, UmbracoObjectTypes.Document);
                return attempt.Success
                    ? new PublicAccessDetailedCacheRefresher.JsonPayload(attempt.Result)
                    : null;
            })
            .WhereNotNull()
            .ToArray();

        HandlePayloads(payloads);
    }
}
