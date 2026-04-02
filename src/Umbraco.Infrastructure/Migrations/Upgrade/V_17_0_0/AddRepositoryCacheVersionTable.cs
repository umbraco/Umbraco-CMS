using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

/// <summary>
/// Migration step that creates the <c>RepositoryCacheVersion</c> table in the database as part of the upgrade to version 17.0.0.
/// </summary>
public class AddRepositoryCacheVersionTable : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddRepositoryCacheVersionTable"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The migration context used to perform migration operations.</param>
    public AddRepositoryCacheVersionTable(IMigrationContext context) : base(context)
    {
    }

    protected override Task MigrateAsync()
    {
        if (TableExists(RepositoryCacheVersionDto.TableName) is false)
        {
            Create.Table<RepositoryCacheVersionDto>().Do();
        }

        return Task.CompletedTask;
    }
}
