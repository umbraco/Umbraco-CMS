using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Adds the template table to the Entity Framework Core model.
/// </summary>
/// <remarks>
/// NPoco creates the table, so the Entity Framework Core migration only advances the model snapshot.
/// </remarks>
public class AddTemplateDto : AsyncMigrationBase
{
    private readonly IEFCoreMigrationExecutor _migrationExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddTemplateDto" /> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="migrationExecutor">The executor that runs the Entity Framework Core migration.</param>
    public AddTemplateDto(
        IMigrationContext context,
        IEFCoreMigrationExecutor migrationExecutor)
        : base(context)
    {
        _migrationExecutor = migrationExecutor;
    }

    /// <inheritdoc />
    protected override async Task MigrateAsync() =>
        await _migrationExecutor.ExecuteSingleMigrationAsync(EFCoreMigration.AddTemplateDto);
}
