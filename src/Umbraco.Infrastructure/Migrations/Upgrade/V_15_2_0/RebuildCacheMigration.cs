using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_2_0;

[Obsolete("Will be removed in V18")]
public class RebuildCacheMigration : MigrationBase
{
    private readonly IDocumentCacheService _documentCacheService;

    public RebuildCacheMigration(IMigrationContext context, IDocumentCacheService documentCacheService) : base(context) =>
        _documentCacheService = documentCacheService;

    protected override void Migrate() =>
        _documentCacheService.ClearMemoryCacheAsync(CancellationToken.None).GetAwaiter().GetResult();
}
