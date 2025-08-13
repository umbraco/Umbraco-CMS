using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <inheritdoc />
public class ElementTreeChangeDistributedCacheNotificationHandler : TreeChangeDistributedCacheNotificationHandlerBase<IElement, ElementTreeChangeNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementTreeChangeDistributedCacheNotificationHandler" /> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public ElementTreeChangeDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    [Obsolete("Scheduled for removal in Umbraco 18.")]
    protected override void Handle(IEnumerable<TreeChange<IElement>> entities)
        => Handle(entities, new Dictionary<string, object?>());

    /// <inheritdoc />
    protected override void Handle(IEnumerable<TreeChange<IElement>> entities, IDictionary<string, object?> state)
        => _distributedCache.RefreshElementCache(entities);
}
