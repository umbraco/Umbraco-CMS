using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Brings the Entity Framework Core model snapshot in line with the element tables.
/// </summary>
/// <remarks>
/// The element tables already exist; the Entity Framework Core migration this advances to is a no-op that only
/// carries the updated model snapshot.
/// </remarks>
public class AddElementDtos : AsyncMigrationBase
{
    private readonly IEFCoreMigrationExecutor _migrationExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddElementDtos" /> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="migrationExecutor">The executor that runs the Entity Framework Core migration.</param>
    public AddElementDtos(
        IMigrationContext context,
        IEFCoreMigrationExecutor migrationExecutor)
        : base(context)
    {
        _migrationExecutor = migrationExecutor;
    }

    /// <inheritdoc />
    protected override async Task MigrateAsync() =>
        await _migrationExecutor.ExecuteSingleMigrationAsync(EFCoreMigration.AddElementDtos);
}
