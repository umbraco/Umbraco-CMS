using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Brings the Entity Framework Core model up to the state the document repository expects.
/// </summary>
/// <remarks>
/// NPoco creates the tables, so the Entity Framework Core migrations this advances to are no-ops that carry the
/// model snapshot. Migrating to the newest one applies every migration a database has not yet recorded,
/// whichever release it upgraded from.
/// </remarks>
public class ReconcileDocumentRepositoryModel : AsyncMigrationBase
{
    private readonly IEFCoreMigrationExecutor _migrationExecutor;

    public ReconcileDocumentRepositoryModel(
        IMigrationContext context,
        IEFCoreMigrationExecutor migrationExecutor)
        : base(context)
    {
        _migrationExecutor = migrationExecutor;
    }

    protected override async Task MigrateAsync() =>
        await _migrationExecutor.ExecuteSingleMigrationAsync(EFCoreMigration.ReconcileDocumentRepositoryModel);
}
