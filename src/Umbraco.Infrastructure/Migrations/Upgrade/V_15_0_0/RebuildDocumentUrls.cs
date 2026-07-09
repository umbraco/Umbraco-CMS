using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0;

/// <summary>
/// Represents a migration step that rebuilds document URLs as part of the upgrade process to version 15.0.0.
/// </summary>
[Obsolete("Remove in Umbraco 18.")]
public class RebuildDocumentUrls : MigrationBase
{
    private readonly IKeyValueService _keyValueService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RebuildDocumentUrls"/> class, used to rebuild document URLs during the upgrade process to version 15.0.0.
    /// </summary>
    /// <param name="context">The migration context providing state and services for the migration.</param>
    /// <param name="documentUrlService">The service used to manage and rebuild document URLs.</param>
    public RebuildDocumentUrls(IMigrationContext context, IDocumentUrlService documentUrlService)
        : this(
              context,
              documentUrlService,
              StaticServiceProvider.Instance.GetRequiredService<IKeyValueService>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RebuildDocumentUrls"/> class, used to rebuild document URLs during the upgrade to version 15.0.0.
    /// </summary>
    /// <param name="context">The migration context for the upgrade process.</param>
    /// <param name="documentUrlService">Service used to manage and rebuild document URLs.</param>
    /// <param name="keyValueService">Service for accessing key-value storage during migration.</param>
    public RebuildDocumentUrls(IMigrationContext context, IDocumentUrlService documentUrlService, IKeyValueService keyValueService)
        : base(context)
    {
        // The documentUrlService parameter is kept for backward compatibility and to maintain
        // constructor signature compatibility with earlier versions/DI registrations. It is not
        // required by this migration, which only needs access to IKeyValueService.
        _ = documentUrlService;
        _keyValueService = keyValueService;
    }

    /// <inheritdoc/>
    protected override void Migrate() =>

        // Clear any existing key to force rebuild on first normal startup.
        // This ensures URL generation runs when all services are fully initialized,
        // rather than during migration when variant content data may not be accessible.
        // See: https://github.com/umbraco/Umbraco-CMS/issues/21337
        _keyValueService.SetValue(DocumentUrlService.RebuildKey, string.Empty);
}
