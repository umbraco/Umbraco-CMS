using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

/// <summary>
/// Migration class responsible for updating the Umbraco OpenIddict integration to version 5.
/// </summary>
public class UpdateToOpenIddictV5 : MigrationBase
{
    private readonly IEFCoreMigrationExecutor _efCoreMigrationExecutor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateToOpenIddictV5"/> class.
    /// </summary>
    /// <param name="context">The migration context used to manage the migration process.</param>
    /// <param name="efCoreMigrationExecutor">The executor responsible for running EF Core migrations.</param>
    public UpdateToOpenIddictV5(IMigrationContext context, IEFCoreMigrationExecutor efCoreMigrationExecutor)
        : base(context)
    {
        _efCoreMigrationExecutor = efCoreMigrationExecutor;
    }

    protected override void Migrate()
    {
        _efCoreMigrationExecutor.ExecuteSingleMigrationAsync(EFCoreMigration.UpdateOpenIddictToV5);
    }
}
