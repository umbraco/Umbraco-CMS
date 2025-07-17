using Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Table;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_16_2_0;

public class AddRepositoryCacheVersionTable : AsyncMigrationBase
{
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
