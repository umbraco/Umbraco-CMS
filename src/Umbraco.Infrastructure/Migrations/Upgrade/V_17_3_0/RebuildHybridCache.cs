using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_3_0;

/// <summary>
/// Clears and re-seeds the HybridCache so that all entries are tagged with the content type ID.
/// This is required for the content type tag-based eviction introduced in 17.3.
/// </summary>
public class RebuildHybridCache : AsyncMigrationBase
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IMediaCacheService _mediaCacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RebuildHybridCache"/> class.
    /// </summary>
    public RebuildHybridCache(
        IMigrationContext context,
        IDocumentCacheService documentCacheService,
        IMediaCacheService mediaCacheService)
        : base(context)
    {
        _documentCacheService = documentCacheService;
        _mediaCacheService = mediaCacheService;
    }

    protected override async Task MigrateAsync()
    {
        await _documentCacheService.ClearMemoryCacheAsync(CancellationToken.None);
        await _mediaCacheService.ClearMemoryCacheAsync(CancellationToken.None);
    }
}
