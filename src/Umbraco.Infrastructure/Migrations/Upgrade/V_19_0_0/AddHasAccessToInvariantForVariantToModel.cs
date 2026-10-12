using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Brings the Entity Framework Core model snapshot in line with the user group invariant-for-variant permission column.
/// </summary>
/// <remarks>
/// <see cref="AddHasAccessToInvariantForVariantColumnToUserGroup" /> adds the column before the main plan runs; the
/// Entity Framework Core migration this advances to is a no-op that only carries the updated model snapshot.
/// </remarks>
public class AddHasAccessToInvariantForVariantToModel : AsyncMigrationBase
{
    private readonly IEFCoreMigrationExecutor _migrationExecutor;

    public AddHasAccessToInvariantForVariantToModel(
        IMigrationContext context,
        IEFCoreMigrationExecutor migrationExecutor)
        : base(context)
    {
        _migrationExecutor = migrationExecutor;
    }

    protected override async Task MigrateAsync() =>
        await _migrationExecutor.ExecuteSingleMigrationAsync(EFCoreMigration.AddHasAccessToInvariantForVariantToModel);
}
