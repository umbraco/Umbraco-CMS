using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_2_0;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

[Obsolete("Remove in Umbraco 18.")]
public class RebuildDocumentUrls : MigrationBase
{
    private readonly IDocumentUrlService _documentUrlService;

    public RebuildDocumentUrls(IMigrationContext context, IDocumentUrlService documentUrlService)
        : base(context) =>
        _documentUrlService = documentUrlService;

    protected override void Migrate()
    {
        // The document URL service requires a write lock that was introduced in a later migration (16.2).
        // We need to add it here as without it the document URL initialization running in this preceding migration step could fail if URLs
        // are determined as requiring a rebuild.
        AddDocumentUrlLock.CreateDocumentUrlsLock(Database);

        _documentUrlService.InitAsync(false, CancellationToken.None).GetAwaiter().GetResult();
    }
}
