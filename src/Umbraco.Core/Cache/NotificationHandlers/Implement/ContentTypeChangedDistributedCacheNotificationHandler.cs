using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
public sealed class ContentTypeChangedDistributedCacheNotificationHandler : ContentTypeChangedDistributedCacheNotificationHandlerBase<IContentType, ContentTypeChangedNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeChangedDistributedCacheNotificationHandler" /> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public ContentTypeChangedDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    protected override void Handle(IEnumerable<ContentTypeChange<IContentType>> entities)
        => _distributedCache.RefreshContentTypeCache(entities);
}
