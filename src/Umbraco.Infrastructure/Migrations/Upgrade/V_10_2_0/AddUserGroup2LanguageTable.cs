using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_2_0;

public class AddUserGroup2LanguageTable : MigrationBase
{
    public AddUserGroup2LanguageTable(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        IEnumerable<string> tables = SqlSyntax.GetTablesInSchema(Context.Database);

        Create.Column("hasAccessToAllLanguages")
            .OnTable(Constants.DatabaseSchema.Tables.UserGroup)
            .AsBoolean()
            .WithDefaultValue(true)
            .Do();

        if (tables.InvariantContains(UserGroup2LanguageDto.TableName))
        {
            return;
        }

        Create.Table<UserGroup2LanguageDto>().Do();
    }
}
