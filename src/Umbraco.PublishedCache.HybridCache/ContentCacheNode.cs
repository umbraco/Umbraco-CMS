using System.ComponentModel;

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
}
