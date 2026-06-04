using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Cache;

/// <summary>
/// Invalidates element caches when an element container (folder) is deleted, so that its key→id mapping
/// is evicted from <see cref="Services.IIdKeyMap"/> on every server.
/// </summary>
/// <remarks>
/// Element container deletions only publish <see cref="EntityContainerDeletedNotification"/> and an
/// <see cref="ElementTreeChangeNotification"/> for the contained elements - never for the container node
/// itself. Without this handler the container's stale mapping survives until the next app restart, and a
/// container recreated under the same key resolves to the old id, so the element tree's children query
/// returns nothing (see #23072).
/// </remarks>
public sealed class ElementContainerDeletedDistributedCacheNotificationHandler
    : DeletedDistributedCacheNotificationHandlerBase<EntityContainer, EntityContainerDeletedNotification>
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementContainerDeletedDistributedCacheNotificationHandler"/> class.
    /// </summary>
    /// <param name="distributedCache">The distributed cache.</param>
    public ElementContainerDeletedDistributedCacheNotificationHandler(DistributedCache distributedCache)
        => _distributedCache = distributedCache;

    /// <inheritdoc />
    protected override void Handle(IEnumerable<EntityContainer> entities, IDictionary<string, object?> state)
    {
        EntityContainer[] elementContainers = entities
            .Where(container => container.ContainedObjectType == Constants.ObjectTypes.Element)
            .ToArray();

        if (elementContainers.Length == 0)
        {
            return;
        }

        _distributedCache.RefreshElementCache(elementContainers);
    }
}
