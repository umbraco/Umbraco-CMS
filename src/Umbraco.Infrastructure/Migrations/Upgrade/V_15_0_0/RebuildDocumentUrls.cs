using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

[Obsolete("Remove in Umbraco 18.")]
public class RebuildDocumentUrls : MigrationBase
{
    private readonly IKeyValueService _keyValueService;

    public RebuildDocumentUrls(IMigrationContext context, IKeyValueService keyValueService)
        : base(context) =>
        _keyValueService = keyValueService;

    protected override void Migrate()
    {
        // Clear any existing key to force rebuild on first normal startup.
        // This ensures URL generation runs when all services are fully initialized,
        // rather than during migration when variant content data may not be accessible.
        // See: https://github.com/umbraco/Umbraco-CMS/issues/21337
        _keyValueService.SetValue(DocumentUrlService.RebuildKey, string.Empty);
    }
}
