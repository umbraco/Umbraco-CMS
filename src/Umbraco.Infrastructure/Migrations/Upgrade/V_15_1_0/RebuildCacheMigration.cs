using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_1_0;

/// <summary>
/// Represents a migration that rebuilds the cache as part of the upgrade process to Umbraco version 15.1.0.
/// </summary>
[Obsolete("Scheduled for removal in Umbraco 18.")]
public class RebuildCacheMigration : MigrationBase
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IMediaCacheService _mediaCacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_1_0.RebuildCacheMigration"/> class.
    /// </summary>
    /// <param name="context">The migration context. <see cref="Umbraco.Cms.Infrastructure.Migrations.IMigrationContext"/></param>
    /// <param name="documentCacheService">The document cache service. <see cref="Umbraco.Cms.Core.Cache.IDocumentCacheService"/></param>
    /// <param name="mediaCacheService">The media cache service. <see cref="Umbraco.Cms.Core.Cache.IMediaCacheService"/></param>
    public RebuildCacheMigration(IMigrationContext context, IDocumentCacheService documentCacheService, IMediaCacheService mediaCacheService) : base(context)
    {
        _documentCacheService = documentCacheService;
        _mediaCacheService = mediaCacheService;
    }

    protected override void Migrate()
    {
        _documentCacheService.ClearMemoryCacheAsync(CancellationToken.None).GetAwaiter().GetResult();
        _mediaCacheService.ClearMemoryCacheAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

}
