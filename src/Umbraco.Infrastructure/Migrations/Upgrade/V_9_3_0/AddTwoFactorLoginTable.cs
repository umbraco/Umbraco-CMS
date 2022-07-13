using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_3_0;

public class AddTwoFactorLoginTable : MigrationBase
{
    public AddTwoFactorLoginTable(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        IEnumerable<string> tables = SqlSyntax.GetTablesInSchema(Context.Database);
        if (tables.InvariantContains(TwoFactorLoginDto.TableName))
        {
            return;
        }

        Create.Table<TwoFactorLoginDto>().Do();
    }
}
