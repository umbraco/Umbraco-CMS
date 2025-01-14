using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_1_0;

[Obsolete("Will be removed in V18")]
public class RebuildCacheMigration : MigrationBase
{
    private readonly IDocumentCacheService _documentCacheService;
    private readonly IMediaCacheService _mediaCacheService;

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
