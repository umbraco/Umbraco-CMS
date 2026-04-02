using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

/// <summary>
/// Executes a migration that updates the Umbraco OpenIddict integration to version 7.
/// </summary>
public class UpdateToOpenIddictV7 : MigrationBase
{
    private readonly IEFCoreMigrationExecutor _efCoreMigrationExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0.UpdateToOpenIddictV7"/> class.
    /// </summary>
    /// <param name="context">The migration context used for managing the migration process.</param>
    /// <param name="efCoreMigrationExecutor">The executor responsible for running EF Core migrations.</param>
    public UpdateToOpenIddictV7(IMigrationContext context, IEFCoreMigrationExecutor efCoreMigrationExecutor)
        : base(context)
    {
        _efCoreMigrationExecutor = efCoreMigrationExecutor;
    }

    protected override void Migrate()
    {
        _efCoreMigrationExecutor.ExecuteSingleMigrationAsync(EFCoreMigration.UpdateOpenIddictToV7);
    }
}
