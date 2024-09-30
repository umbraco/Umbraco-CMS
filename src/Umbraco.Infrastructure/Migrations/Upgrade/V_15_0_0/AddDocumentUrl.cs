using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

[Obsolete("Remove in Umbraco 18.")]
public class AddDocumentUrl : MigrationBase
{
    private readonly IDocumentUrlService _documentUrlService;

    public AddDocumentUrl(IMigrationContext context, IDocumentUrlService documentUrlService)
        : base(context)
    {
        _documentUrlService = documentUrlService;
    }

    protected override void Migrate()
    {
        Create.Table<DocumentUrlDto>().Do();
        _documentUrlService.InitAsync(false, CancellationToken.None);
    }
}
