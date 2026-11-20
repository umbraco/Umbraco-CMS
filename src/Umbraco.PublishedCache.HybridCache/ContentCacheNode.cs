using System.ComponentModel;
using Umbraco.Cms.Infrastructure.HybridCache.Serialization;

namespace Umbraco.Cms.Infrastructure.HybridCache;

// This is for cache performance reasons, see https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid?view=aspnetcore-9.0#reuse-objects
[ImmutableObject(true)]
public sealed class ContentCacheNode
{
    public int Id { get; set; }

    public Guid Key { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreateDate { get; set; }

    public int CreatorId { get; set; }

    public int ContentTypeId { get; set; }

    public bool IsDraft { get; set; }

    public ContentData? Data { get; set; }

    /// <summary>
    /// Gets or sets the type of entity the node was built for.
    /// </summary>
    /// <remarks>
    /// Documents, media and elements are cached under their keys in one HybridCache, so a reader checks this to
    /// tell whether an entry is one of its own. It is unset on nodes cached before it was recorded.
    /// </remarks>
    public ContentCacheDataSerializerEntityType EntityType { get; set; }

    /// <summary>
    /// Gets a value indicating whether the node is known to have been built for an entity type other than the given one.
    /// </summary>
    /// <param name="entityType">The entity type the reader caches.</param>
    /// <returns><c>true</c> when <see cref="EntityType"/> is set and differs from <paramref name="entityType"/>.</returns>
    internal bool IsOtherEntityType(ContentCacheDataSerializerEntityType entityType)
        => EntityType != default && EntityType != entityType;
}
