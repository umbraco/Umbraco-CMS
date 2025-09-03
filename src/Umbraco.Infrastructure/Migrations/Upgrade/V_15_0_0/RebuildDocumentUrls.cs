using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

[Obsolete("Remove in Umbraco 18.")]
public class RebuildDocumentUrls : MigrationBase
{
    private readonly IDocumentUrlService _documentUrlService;

    public RebuildDocumentUrls(IMigrationContext context, IDocumentUrlService documentUrlService)
        : base(context) =>
        _documentUrlService = documentUrlService;

    protected override void Migrate()
        => _documentUrlService.InitAsync(false, CancellationToken.None).GetAwaiter().GetResult();
}
